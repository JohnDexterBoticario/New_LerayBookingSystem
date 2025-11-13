using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using New_LeRayBookingSystem.Data;
using New_LeRayBookingSystem.Models;
using New_LeRayBookingSystem.Services; // Import EmailService namespace

namespace New_LeRayBookingSystem.Controllers
{
    public class FeedbacksController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public FeedbacksController(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        // GET: Feedbacks
        public async Task<IActionResult> Index()
        {
            var feedbacks = _context.Feedbacks.Include(f => f.User);
            return View(await feedbacks.ToListAsync());
        }

        // GET: Feedbacks/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var feedback = await _context.Feedbacks
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (feedback == null) return NotFound();

            return View(feedback);
        }

        // GET: Feedbacks/Create
        public IActionResult Create()
        {
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName");
            return View();
        }

        // POST: Feedbacks/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Description,UserId")] Feedback feedback)
        {
            if (ModelState.IsValid)
            {
                feedback.Id = Guid.NewGuid().ToString();
                feedback.CreatedAt = DateTime.Now;
                feedback.UpdatedAt = DateTime.Now;

                _context.Add(feedback);
                await _context.SaveChangesAsync();

                // Send confirmation email to user
                var user = await _context.Users.FindAsync(feedback.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var subject = "Thank you for your feedback!";
                    var body = $@"
                        Hello {user.FullName},<br/><br/>
                        Thank you for submitting your feedback:<br/>
                        <blockquote>{feedback.Description}</blockquote>
                        We appreciate your input.<br/><br/>
                        Regards,<br/>
                        LeRay Booking System";

                    await _emailService.SendEmailAsync(user.Email, subject, body);
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", feedback.UserId);
            return View(feedback);
        }

        // GET: Feedbacks/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback == null) return NotFound();

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", feedback.UserId);
            return View(feedback);
        }

        // POST: Feedbacks/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("Id,Description,UserId,CreatedAt,UpdatedAt")] Feedback feedback)
        {
            if (id != feedback.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    feedback.UpdatedAt = DateTime.Now;
                    _context.Update(feedback);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FeedbackExists(feedback.Id)) return NotFound();
                    throw;
                }
                return RedirectToAction(nameof(Index));
            }

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", feedback.UserId);
            return View(feedback);
        }

        // GET: Feedbacks/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var feedback = await _context.Feedbacks
                .Include(f => f.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (feedback == null) return NotFound();

            return View(feedback);
        }

        // POST: Feedbacks/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var feedback = await _context.Feedbacks.FindAsync(id);
            if (feedback != null)
            {
                _context.Feedbacks.Remove(feedback);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool FeedbackExists(string id)
        {
            return _context.Feedbacks.Any(e => e.Id == id);
        }
    }
}
