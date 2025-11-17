using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using New_LeRayBookingSystem.Data;
using New_LeRayBookingSystem.Models;
using New_LeRayBookingSystem.Models.DTOs;
using New_LeRayBookingSystem.Services;

namespace New_LeRayBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _audit;

        public ServicesController(ApplicationDbContext context, IAuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        // GET: api/Services
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var services = await _context.CustomerServices.ToListAsync();

            await _audit.LogAsync(
                // match your IAuditService signature (action,module,entityId,...). 
                // I'm using the 6-arg signature you had earlier: (userId, action, module, description, ipAddress, userAgent)
                User?.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous" : "Anonymous",
                "Viewed",
                "Services",
                "Viewed all customer services",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                Request.Headers["User-Agent"].ToString()
            );

            return Ok(services);
        }

        // GET: api/Services/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var service = await _context.CustomerServices.FindAsync(id);
            if (service == null) return NotFound();

            await _audit.LogAsync(
                User?.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous" : "Anonymous",
                "Viewed",
                "Services",
                $"Viewed customer service ID {id}",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                Request.Headers["User-Agent"].ToString()
            );

            return Ok(service);
        }

        // POST: api/Services
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateServiceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var service = new CustomerService
            {
                ServiceName = dto.ServiceName,
                Category = dto.Category,
                Price = dto.Price,
                Duration = TimeSpan.FromMinutes(dto.DurationMinutes),
                IsBundle = dto.IsBundle,
                Description = dto.Description
            };

            _context.CustomerServices.Add(service);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(
                User?.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous" : "Anonymous",
                "Created",
                "Services",
                $"Created customer service '{service.ServiceName}' (ID {service.Id})",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                Request.Headers["User-Agent"].ToString()
            );

            return CreatedAtAction(nameof(Get), new { id = service.Id }, service);
        }

        // PUT: api/Services/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateServiceDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var service = await _context.CustomerServices.FindAsync(id);
            if (service == null) return NotFound();

            service.ServiceName = dto.ServiceName;
            service.Category = dto.Category;
            service.Price = dto.Price;
            service.Duration = TimeSpan.FromMinutes(dto.DurationMinutes);
            service.IsBundle = dto.IsBundle;
            service.Description = dto.Description;

            await _context.SaveChangesAsync();

            await _audit.LogAsync(
                User?.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous" : "Anonymous",
                "Updated",
                "Services",
                $"Updated customer service ID {id} ({service.ServiceName})",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                Request.Headers["User-Agent"].ToString()
            );

            return NoContent();
        }

        // DELETE: api/Services/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var service = await _context.CustomerServices.FindAsync(id);
            if (service == null) return NotFound();

            _context.CustomerServices.Remove(service);
            await _context.SaveChangesAsync();

            await _audit.LogAsync(
                User?.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "Anonymous" : "Anonymous",
                "Deleted",
                "Services",
                $"Deleted customer service ID {id} ({service.ServiceName})",
                HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown",
                Request.Headers["User-Agent"].ToString()
            );

            return NoContent();
        }
    }
}
