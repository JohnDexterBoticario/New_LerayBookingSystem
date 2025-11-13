using New_LeRayBookingSystem.Data;
using New_LeRayBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using New_LeRayBookingSystem.Services;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace New_LeRayBookingSystem.Controllers
{
    public class AuditLogsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        public AuditLogsController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: AuditLogs
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.AuditLogs.Include(a => a.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: AuditLogs/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditLog = await _context.AuditLogs
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auditLog == null)
            {
                return NotFound();
            }

            return View(auditLog);
        }

        // GET: AuditLogs/Create
        public IActionResult Create()
        {
            // FIX: Show 'FullName' not 'Id'
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName");
            return View();
        }

        // POST: AuditLogs/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Action,UserId,Timestamp,EntityType,EntityId,Details")] AuditLog auditLog)
        {
            if (ModelState.IsValid)
            {
                _context.Add(auditLog);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            // FIX: Show 'FullName' not 'Id'
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", auditLog.UserId);
            return View(auditLog);
        }

        // GET: AuditLogs/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditLog = await _context.AuditLogs.FindAsync(id);
            if (auditLog == null)
            {
                return NotFound();
            }
            // FIX: Show 'FullName' not 'Id'
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", auditLog.UserId);
            return View(auditLog);
        }

        // POST: AuditLogs/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Action,UserId,Timestamp,EntityType,EntityId,Details")] AuditLog auditLog)
        {
            if (id != auditLog.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(auditLog);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AuditLogExists(auditLog.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            // FIX: Show 'FullName' not 'Id'
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", auditLog.UserId);
            return View(auditLog);
        }

        // ... (Delete methods are unchanged) ...
        // GET: AuditLogs/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var auditLog = await _context.AuditLogs
                .Include(a => a.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (auditLog == null)
            {
                return NotFound();
            }

            return View(auditLog);
        }

        // POST: AuditLogs/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var auditLog = await _context.AuditLogs.FindAsync(id);
            if (auditLog != null)
            {
                _context.AuditLogs.Remove(auditLog);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool AuditLogExists(string id)
        {
            return _context.AuditLogs.Any(e => e.Id == id);
        }
    }
}