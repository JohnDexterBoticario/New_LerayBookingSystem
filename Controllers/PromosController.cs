using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using New_LeRayBookingSystem.Data;
using New_LeRayBookingSystem.Models;
using Microsoft.AspNetCore.Identity;

namespace New_LeRayBookingSystem.Controllers
{
    public class PromosController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PromosController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Promos
        public async Task<IActionResult> Index()
        {
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

            return View(promo);
        }

        // GET: Promos/Create
        public IActionResult Create()
        {
            var promo = new Promos
            {
                PromoCode = GeneratePromoCode(), // ✅ auto-generate 4-char promo code
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30), // default 30 days validity
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

                // ✅ Auto-assign logged-in user as creator
                promo.CreatedBy = _userManager.GetUserId(User);

                // ✅ Ensure PromoCode exists
                if (string.IsNullOrWhiteSpace(promo.PromoCode))
                    promo.PromoCode = GeneratePromoCode();

                _context.Add(promo);
                await _context.SaveChangesAsync();

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
            }
            return RedirectToAction(nameof(Index));
        }

        private bool PromosExists(int id)
        {
            return _context.Promos.Any(e => e.Id == id);
        }

        // ✅ Helper: Generate 4-character random promo code with "LeRay-" prefix
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
