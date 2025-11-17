using System.Security.Claims;
using System.IO;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using New_LeRayBookingSystem.Data;
using New_LeRayBookingSystem.Models;
using New_LeRayBookingSystem.Models.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace New_LeRayBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingsController> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BookingsController(
            ApplicationDbContext context,
            ILogger<BookingsController> logger,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        // ----------------------------
        // HELPERS
        // ----------------------------
        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException("User ID not found");

        private string GetIpAddress() =>
            HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";

        // ----------------------------
        // GET: api/bookings/my-bookings
        // ----------------------------
        [HttpGet("my-bookings")]
        public async Task<ActionResult<IEnumerable<BookingDetailsDto>>> GetUserBookings()
        {
            var userId = GetUserId();

            var bookings = await _context.Appointments
                .Where(b => b.UserId == userId && b.AppointmentDate > DateTime.UtcNow)
                .Include(b => b.CustomerService)
                .OrderBy(b => b.AppointmentDate)
                .Select(b => new BookingDetailsDto
                {
                    Id = b.Id,
                    StartTime = b.AppointmentDate,
                    EndTime = b.CustomerService != null
                        ? b.AppointmentDate.Add(b.CustomerService.Duration)
                        : b.AppointmentDate,
                    Status = b.Status,
                    Notes = b.Notes,
                    Service = b.CustomerService == null ? null : new ServiceDto
                    {
                        Id = b.CustomerService.Id,
                        Name = b.CustomerService.ServiceName,
                        Description = b.CustomerService.Description,
                        Price = b.CustomerService.Price,
                        DurationInMinutes = (int)b.CustomerService.Duration.TotalMinutes
                    }
                })
                .ToListAsync();

            return Ok(bookings);
        }

        // ----------------------------
        // GET: api/bookings/{id}
        // ----------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<BookingDetailsDto>> GetBookingById(int id)
        {
            var userId = GetUserId();

            var b = await _context.Appointments
                .Include(x => x.CustomerService)
                .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);

            if (b == null) return NotFound();

            var dto = new BookingDetailsDto
            {
                Id = b.Id,
                StartTime = b.AppointmentDate,
                EndTime = b.CustomerService != null
                    ? b.AppointmentDate.Add(b.CustomerService.Duration)
                    : b.AppointmentDate,
                Status = b.Status,
                Notes = b.Notes,
                Service = (b.CustomerService == null) ? null : new ServiceDto
                {
                    Id = b.CustomerService.Id,
                    Name = b.CustomerService.ServiceName,
                    Description = b.CustomerService.Description,
                    Price = b.CustomerService.Price,
                    DurationInMinutes = (int)b.CustomerService.Duration.TotalMinutes
                }
            };

            return Ok(dto);
        }

        // ----------------------------
        // POST: api/bookings (Create Booking)
        // ----------------------------
        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromForm] CreateBookingDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetUserId();
            var ip = GetIpAddress();

            // ----------------------------
            // FETCH SERVICE PROPERLY
            // ----------------------------
            if (!int.TryParse(dto.ServiceId, out int serviceId))
                return BadRequest("Invalid service ID format.");

            var service = await _context.CustomerServices
                .FirstOrDefaultAsync(s => s.Id == serviceId);

            if (service == null)
            {
                await LogAudit("CreateFailed", userId, ip, "Booking",
                    $"Invalid service id {dto.ServiceId}", "");

                return BadRequest($"Service not found (ID = {dto.ServiceId}).");
            }

            // ----------------------------
            // DATE + TIME VALIDATION
            // ----------------------------
            var startTime = dto.StartTime;
            if (startTime == DateTime.MinValue)
            {
                await LogAudit("CreateFailed", userId, ip, "Booking",
                    "Invalid booking date", "");

                return BadRequest("Invalid booking date.");
            }

            var endTime = startTime.Add(service.Duration);

            // ----------------------------
            // TIME CONFLICT CHECK
            // ----------------------------
            // ---------------------------------------------
// SAFE TIME CONFLICT CHECK (no EF translation)
// ---------------------------------------------
var existing = await _context.Appointments
    .Where(b =>
        b.Status == "Confirmed" &&
        b.ServiceId == service.Id)
    .Include(b => b.CustomerService)
    .ToListAsync();   // <-- switch to in-memory (required)

bool overlapping = existing.Any(b =>
{
    var existingStart = b.AppointmentDate;
    var existingEnd = existingStart.Add(b.CustomerService.Duration);

    return startTime < existingEnd && endTime > existingStart;
});

if (overlapping)
{
    await LogAudit("TimeSlotConflict", userId, ip, "Booking",
        $"Time conflict for service {service.Id} at {startTime:o}", "");

    return Conflict("Time slot unavailable.");
}


            // ----------------------------
            // PAYMENT FILE (REQUIRED)
            // ----------------------------
            if (dto.PaymentReceiptFile == null || dto.PaymentReceiptFile.Length == 0)
                return BadRequest("Payment receipt is required.");

            var uploadsFolder = Path.Combine(
                _webHostEnvironment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                "receipts");

            Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString("N") + "_" + dto.PaymentReceiptFile.FileName;
            var filePath = Path.Combine(uploadsFolder, fileName);

            await using (var fs = new FileStream(filePath, FileMode.Create))
                await dto.PaymentReceiptFile.CopyToAsync(fs);

            string receiptPath = $"/receipts/{fileName}";

            // ----------------------------
            // CREATE BOOKING
            // ----------------------------
            var booking = new Appointment
            {
                UserId = userId,
                ServiceId = service.Id,
                AppointmentDate = startTime,
                Status = "Pending Verification",
                Notes = dto.Notes,
                ServiceName = service.ServiceName,
                PaymentMethod = dto.PaymentMethod ?? "Online",
                PaymentStatus = "Pending Verification",
                PaymentReceiptPath = receiptPath,
                CreatedBy = userId,
                UpdatedBy = userId,
                LastUpdatedBy = userId
            };

            await _context.Appointments.AddAsync(booking);
            await _context.SaveChangesAsync();

            // ----------------------------
            // LINK SERVICE VIA AppointmentService
            // ----------------------------
            var serviceEntry = new AppointmentService
            {
                AppointmentId = booking.Id,
                ServiceId = service.Id,
                Price = service.Price,
                Quantity = 1
            };

            await _context.AppointmentServices.AddAsync(serviceEntry);
            await _context.SaveChangesAsync();

            // ----------------------------
            // AUDIT SUCCESS
            // ----------------------------
            var summary = new
            {
                BookingId = booking.Id,
                ServiceName = service.ServiceName,
                Start = booking.AppointmentDate,
                End = booking.AppointmentDate.Add(service.Duration),
                Receipt = receiptPath
            };

            await LogAudit("Create", userId, ip, "Booking",
                $"Created booking {booking.Id}", booking.Id.ToString(),
                JsonSerializer.Serialize(summary));

            // ----------------------------
            // RESULT DTO
            // ----------------------------
            var result = new BookingDetailsDto
            {
                Id = booking.Id,
                StartTime = booking.AppointmentDate,
                EndTime = booking.AppointmentDate.Add(service.Duration),
                Status = booking.Status,
                Notes = booking.Notes,
                Service = new ServiceDto
                {
                    Id = service.Id,
                    Name = service.ServiceName,
                    Description = service.Description,
                    Price = service.Price,
                    DurationInMinutes = (int)service.Duration.TotalMinutes
                }
            };

            return CreatedAtAction(nameof(GetBookingById), new { id = booking.Id }, result);
        }

        // ----------------------------
        // AUDIT HELPER
        // ----------------------------
        private async Task LogAudit(string action, string userId, string ip,
            string module, string description,
            string? entityId = "", string? details = "")
        {
            var log = new AuditLog
            {
                Action = action,
                UserId = userId,
                Module = module,
                Description = description,
                IpAddress = ip,
                EntityType = module,
                EntityId = entityId ?? "",
                Details = details ?? ""
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
