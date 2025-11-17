using New_LeRayBookingSystem.Data;
using New_LeRayBookingSystem.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace New_LeRayBookingSystem.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _db;
        private readonly IHttpContextAccessor _http;
        
        public AuditService(ApplicationDbContext db, IHttpContextAccessor http)
        {
            _db = db;
            _http = http;
        }
        
        // Fixed: Method signature now matches the interface
        public async Task LogAsync(string action, string entityType, string entityId, string details, string module, string description)
        {
            var user = _http.HttpContext?.User;
            
            var log = new AuditLog
            {
                UserId = user?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Guest",
                Action = action,
                Module = module,
                Description = description,
                IpAddress = _http.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown",
                EntityType = entityType,
                EntityId = entityId,
                Details = details,
                Timestamp = DateTime.UtcNow
                
            };
            
            _db.AuditLogs.Add(log);
            await _db.SaveChangesAsync();
        }
    }
}