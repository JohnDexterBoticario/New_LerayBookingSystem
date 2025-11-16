using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using New_LeRayBookingSystem.Data;

namespace New_LeRayBookingSystem.Controllers
{
    public class AuditLogsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AuditLogsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: AuditLogs
        public async Task<IActionResult> Index()
        {
            var logs = await _context.AuditLogs
                .OrderByDescending(a => a.Timestamp)
                .ToListAsync();

            return View(logs);
        }

        // GET: AuditLogs/Details/5
        public async Task<IActionResult> Details(string? id)  // Changed from int? to string?
        {
            if (id == null) return NotFound();
            
            var auditLog = await _context.AuditLogs
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (auditLog == null) return NotFound();
            
            return View(auditLog);
        }

        // Read-only: Remove any create/edit/delete
        // These actions should not exist for audit logs
    }
}
