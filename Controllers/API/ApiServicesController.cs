using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using New_LeRayBookingSystem.Data;

namespace New_LeRayBookingSystem.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApiServicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ApiServicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/ApiServices
        [HttpGet]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _context.CustomerServices
                .Select(s => new
                {
                    id = s.Id,
                    serviceName = s.ServiceName
                })
                .ToListAsync();

            return Ok(services);
        }
    }
}
