using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using New_LeRayBookingSystem.Data;
using New_LeRayBookingSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using New_LeRayBookingSystem.Services;

namespace New_LeRayBookingSystem.Controllers
{
    [Authorize]
    public class PromosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _audit;

        public PromosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IAuditService audit)
        {
            _context = context;
            _userManager = userManager;
            _audit = audit;
        }

        private string GetUserId() =>
            User?.Identity?.IsAuthenticated == true
                ? _userManager.GetUserId(User)
                : "Anonymous";

        private string IP() =>
            HttpContext.Connection.RemoteIpAddress?.ToString();

        private string UA() =>
            Request.Headers["User-Agent"].ToString();

        // GET: Promos
        public async Task<IActionResult> Index()
        {
            await _audit.LogAsync(
                GetUserId(),
                "Viewed",
                "Promos",
                "Viewed promo list",
                IP(),
                UA()
            );

            var promos = await _context.Promos
                .Include(p => p.CreatedByUser)
                .ToListAsync();

            return View(promos);
        }

        // GET: Promos/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var promo = await _context.Promos
                .Include(p => p.CreatedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (promo == null) return NotFound();

            await _audit.LogAsync(
                GetUserId(),
                "Viewed",
                "Promos",
                $"Viewed details of promo ID {promo.Id}",
                IP(),
                UA()
            );

            return View(promo);
        }

        // GET: Promos/Create
        public IActionResult Create()
        {
            var promo = new Promos
            {
                PromoCode = GeneratePromoCode(),
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                IsActive = true
            };

            return View(promo);
        }

        // POST: Promos/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Title,Description,Price,PromoCode,StartDate,EndDate,IsActive")] Promos promo)
        {
            if (ModelState.IsValid)
            {
                promo.CreatedAt = DateTime.Now;
                promo.CreatedBy = GetUserId();

                if (string.IsNullOrWhiteSpace(promo.PromoCode))
                    promo.PromoCode = GeneratePromoCode();

                _context.Add(promo);
                await _context.SaveChangesAsync();

                await _audit.LogAsync(
                    GetUserId(),
                    "Created",
                    "Promos",
                    $"Created promo '{promo.Title}' with code {promo.PromoCode}",
                    IP(),
                    UA()
                );

                return RedirectToAction(nameof(Index));
            }

            return View(promo);
        }

        // GET: Promos/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var promo = await _context.Promos.FindAsync(id);
            if (promo == null) return NotFound();

            return View(promo);
        }

        // POST: Promos/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Price,PromoCode,StartDate,EndDate,IsActive,CreatedBy,CreatedAt")] Promos promo)
        {
            if (id != promo.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(promo);
                    await _context.SaveChangesAsync();

                    await _audit.LogAsync(
                        GetUserId(),
                        "Updated",
                        "Promos",
                        $"Updated promo ID {promo.Id} ({promo.Title})",
                        IP(),
                        UA()
                    );
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PromosExists(promo.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            return View(promo);
        }

        // GET: Promos/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var promo = await _context.Promos
                .Include(p => p.CreatedByUser)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (promo == null) return NotFound();

            return View(promo);
        }

        // POST: Promos/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var promo = await _context.Promos.FindAsync(id);
            if (promo != null)
            {
                _context.Promos.Remove(promo);
                await _context.SaveChangesAsync();

                await _audit.LogAsync(
                    GetUserId(),
                    "Deleted",
                    "Promos",
                    $"Deleted promo ID {promo.Id} ({promo.Title})",
                    IP(),
                    UA()
                );
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PromosExists(int id)
        {
            return _context.Promos.Any(e => e.Id == id);
        }

        private string GeneratePromoCode()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            var suffix = new string(Enumerable.Repeat(chars, 4)
                .Select(s => s[random.Next(s.Length)]).ToArray());
            return $"LeRay-{suffix}";
        }
    }
}
