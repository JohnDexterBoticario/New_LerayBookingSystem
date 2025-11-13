using System.Security.Claims;
using System.IO; 
using New_LeRayBookingSystem.Data;
using New_LeRayBookingSystem.Models.DTOs;
using New_LeRayBookingSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting; // REQUIRED for IWebHostEnvironment

namespace New_LeRayBookingSystem.Controllers
{
    // --- START OF API CONTROLLER ---
    // Inject IWebHostEnvironment to correctly access wwwroot folder.
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BookingsController(
        ApplicationDbContext context,
        ILogger<BookingsController> logger,
        IWebHostEnvironment webHostEnvironment) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<BookingsController> _logger = logger;
        private readonly IWebHostEnvironment _webHostEnvironment = webHostEnvironment;

        // Note: Switched to CustomerService? for local variable type consistency
        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier) ??
                                      throw new UnauthorizedAccessException("User ID not found in token");

        [HttpGet("my-bookings")]
        public async Task<ActionResult<IEnumerable<BookingDetailsDto>>> GetUserBookings()
        {
            var userId = GetUserId();

            var bookings = await _context.Appointments
                .Where(b => b.UserId == userId && b.Status == "Confirmed" && b.AppointmentDate > DateTime.UtcNow)
                // Using CustomerService navigation property
                .Include(b => b.CustomerService)
                .Select(b => new BookingDetailsDto
                {
                    Id = b.Id,
                    StartTime = b.AppointmentDate,
                    // Use CustomerService navigation property
                    EndTime = b.CustomerService != null ? b.AppointmentDate.Add(b.CustomerService.Duration) : b.AppointmentDate,
                    Status = b.Status,
                    Notes = b.Notes,
                    Service = (b.CustomerService == null) ? null : new ServiceDto
                    {
                        Id = b.CustomerService!.Id,
                        Name = b.CustomerService.ServiceName,
                        Description = b.CustomerService.Description,
                        Price = b.CustomerService.Price,
                        DurationInMinutes = (int)b.CustomerService.Duration.TotalMinutes
                    }
                })
                .ToListAsync();

            return Ok(bookings);
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromForm] CreateBookingDto createBookingDto)
        {
            // --- 1. Validation Check
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // --- 2. Service and User Retrieval (using CustomerService model) ---
            var userId = GetUserId();
            CustomerService? service = null; // Variable type changed to CustomerService

            if (createBookingDto.ServiceId == "ServiceName")
            {
                // Correctly querying the CustomerServices DbSet
                service = await _context.CustomerServices.FirstOrDefaultAsync(s => s.Category == "ServiceName");
            }
            else
            {
                if (int.TryParse(createBookingDto.ServiceId, out int serviceId))
                {
                    // Correctly querying the CustomerServices DbSet
                    service = await _context.CustomerServices.FirstOrDefaultAsync(s => s.Id == serviceId);
                }
            }

            if (service == null)
            {
                if (createBookingDto.ServiceId == "SERVICE_AGGREGATE")
                {
                    return BadRequest("Booking failed: Service Bundle placeholder configuration is missing. Please ensure a service exists in the 'Bundle' category.");
                }

                return BadRequest($"Invalid service ID ({createBookingDto.ServiceId}) provided. The selected service could not be found.");
            }

            var startTime = createBookingDto.StartTime;

            if (startTime == DateTime.MinValue)
            {
                return BadRequest("Invalid date or time format in the request.");
            }

            var endTime = startTime.Add(service.Duration);

            // ------------------------------------------------------------------
            // --- 3. OVERLAPPING CHECK (CRITICAL FIX FOR LINQ TRANSLATION) ---
            // ------------------------------------------------------------------

            // Step 1: Filter appointments in the database (server-side) by status and service ID.
            // This is the efficient part of the query.
            var confirmedBookingsQuery = _context.Appointments
                .Where(b => b.Status == "Confirmed" && b.ServiceId == service.Id)
                .Include(b => b.CustomerService);

            // Step 2: Fetch results and switch to client-side (C#) evaluation using AsEnumerable().
            // This allows the use of DateTime.Add(), which MySQL provider cannot translate.
            var confirmedBookings = await confirmedBookingsQuery.ToListAsync();

            // Step 3: Perform the overlap check in C# memory.
            var overlappingBooking = confirmedBookings.Any(b =>
                b.CustomerService != null &&

                // Overlap exists if: (New start < Existing end) AND (New end > Existing start)
                (startTime < b.AppointmentDate.Add(b.CustomerService.Duration)) &&
                (endTime > b.AppointmentDate)
            );

            if (overlappingBooking)
            {
                return Conflict("The selected time slot for this service is no longer available.");
            }

            // --- 4. Handle File Upload (REQUIRED STEP) ---

            if (createBookingDto.PaymentReceiptFile == null || createBookingDto.PaymentReceiptFile.Length == 0)
            {
                return BadRequest("Payment receipt file is mandatory for booking.");
            }

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "receipts");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + createBookingDto.PaymentReceiptFile!.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await createBookingDto.PaymentReceiptFile.CopyToAsync(fileStream);
            }

            string receiptPath = $"/receipts/{uniqueFileName}";

            // --- 5. Create Appointment Model
var booking = new Appointment
{
    UserId = userId,
    ServiceId = service.Id,
    AppointmentDate = startTime,
    Status = "Pending Verification",
    Notes = createBookingDto.Notes,

    ServiceName = service.ServiceName,
    PaymentMethod = createBookingDto.PaymentMethod ?? "Online",
    PaymentStatus = "Pending Verification",
    PaymentReceiptPath = receiptPath,

    CreatedBy = userId,
    UpdatedBy = userId,
    LastUpdatedBy = userId,
};

// --- 5b. 🔥 ADD THE JUNCTION TABLE ENTRY (AppointmentService) 🔥 ---
// NOTE: Assuming your junction model is called AppointmentService (singular) 
// and that it is mapped to a DbSet in your ApplicationDbContext.

var appointmentServiceEntry = new AppointmentService // Replace 'AppointmentService' with your actual model name
{
    // The AppointmentId will be populated automatically when saving, but we link the objects first
    Appointment = booking, // Link the new Appointment object
    ServiceId = service.Id, // Link the valid Service ID
    
    // Add other required fields from your table definition
    Price = service.Price, // Use the service price
    Quantity = 1 // Default quantity
};

// Add the junction entity to its DbSet (assuming DbSet is named AppointmentServices)
await _context.AppointmentServices.AddAsync(appointmentServiceEntry);
// -------------------------------------------------------------------

// --- 6. Save to Database
await _context.Appointments.AddAsync(booking);
await _context.SaveChangesAsync();
            _logger.LogInformation("New appointment created: {BookingId} for user {UserId}", booking.Id, userId);

            // --- 7. Return Result
            return CreatedAtAction(nameof(GetUserBookings), new { id = booking.Id }, new BookingDetailsDto
            {
                Id = booking.Id,
                StartTime = booking.AppointmentDate,
                EndTime = endTime,
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
            });
        }
    } 
    // --- END OF API CONTROLLER ---
    
    // The second controller must be outside the first controller's scope.
    public class BookingController : Controller 
    {
        // ... other actions like Index or Details

        [HttpPost]
       public IActionResult SubmitBooking(CreateBookingDto model)// The name might be "Book" or "Create"
        {
            if (ModelState.IsValid)
            {
                // 1. **Your Booking Logic** (e.g., saving data to a database)
                //    db.Bookings.Add(model.Booking);
                //    db.SaveChanges();

                // 2. **🛑 CRITICAL CHANGE HERE 🛑**
                //    INSTEAD OF: return View("Success"); // Which resulted in the 404

                //    USE THIS TO REDIRECT TO THE HOME PAGE:
                return RedirectToAction("Index", "Home");
                // This tells the application: go to the "Index" action in the "HomeController" (your typical homepage)
            }

            // If validation failed, return the user back to the form
            return View(model);
        }
    }
} 