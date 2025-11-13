using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using New_LeRayBookingSystem.Data;
using New_LeRayBookingSystem.Models;
using New_LeRayBookingSystem.Services;

namespace New_LeRayBookingSystem.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, IEmailService emailService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _emailService = emailService;
            _userManager = userManager;
        }

        // ✅ ADMIN / STAFF ACCESS: View all appointments
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Index()
        {
            var appointments = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.CustomerService)
                .Include(a => a.Promo)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(appointments);
        }

        // ✅ CLIENT ACCESS: View only own appointments
        [Authorize(Roles = "Client,Admin,SuperAdmin")]
        public async Task<IActionResult> MyAppointments()
        {
            var userId = _userManager.GetUserId(User);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var myAppointments = await _context.Appointments
                .Include(a => a.CustomerService)
                .Include(a => a.Promo)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();

            return View(myAppointments);
        }

        // GET: Appointments/Details/5
        [Authorize(Roles = "Client,Admin,SuperAdmin")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.CustomerService)
                .Include(a => a.Promo)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (appointment == null)
                return NotFound();

            // Restrict client access to their own appointments only
            if (User.IsInRole("Client") && appointment.UserId != _userManager.GetUserId(User))
                return Forbid();

            return View(appointment);
        }

        // GET: Appointments/Create
        [Authorize(Roles = "Client,Admin,SuperAdmin")]
        public IActionResult Create()
        {
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Title");
            ViewData["PromoId"] = new SelectList(_context.Promos, "Id", "Title");

            // Only Admin can select users manually
            if (User.IsInRole("Admin") || User.IsInRole("SuperAdmin"))
                ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName");

            return View();
        }

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Client,Admin,SuperAdmin")]
        public async Task<IActionResult> Create(Appointment appointment)
        {
            if (!ModelState.IsValid)
            {
                ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Title", appointment.ServiceId);
                ViewData["PromoId"] = new SelectList(_context.Promos, "Id", "Title", appointment.PromoId);
                return View(appointment);
            }

            // Assign current user if client
            if (User.IsInRole("Client"))
            {
                appointment.UserId = _userManager.GetUserId(User)!;
            }

            // System fields
            appointment.PaymentId = Guid.NewGuid().ToString();
            appointment.Status = "Pending";
            appointment.PaymentStatus = "Unpaid";
            appointment.CreatedAt = DateTime.Now;
            appointment.UpdatedAt = DateTime.Now;
            appointment.LastUpdatedAt = DateTime.Now;
            appointment.CreatedBy = appointment.UserId!;
            appointment.UpdatedBy = appointment.UserId!;
            appointment.LastUpdatedBy = appointment.UserId!;

            _context.Add(appointment);
            await _context.SaveChangesAsync();

            // Send confirmation email
            var user = await _context.Users.FindAsync(appointment.UserId);
            var service = await _context.Services.FindAsync(appointment.ServiceId);

            if (user != null && service != null && !string.IsNullOrWhiteSpace(user.Email))
            {
                var subject = "Appointment Confirmation";
                var body = $@"
                    Hello {user.FullName},<br/><br/>
                    Your appointment for <strong>{service.Title}</strong> on 
                    <strong>{appointment.AppointmentDate:MMMM dd, yyyy hh:mm tt}</strong> 
                    has been successfully booked.<br/><br/>
                    Thank you,<br/>LeRay Booking System";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }

            // Redirect based on role
            return User.IsInRole("Admin") || User.IsInRole("SuperAdmin")
                ? RedirectToAction(nameof(Index))
                : RedirectToAction(nameof(MyAppointments));
        }

        // GET: Appointments/Edit/5
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null)
                return NotFound();

            ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", appointment.UserId);
            ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Title", appointment.ServiceId);
            ViewData["PromoId"] = new SelectList(_context.Promos, "Id", "Title", appointment.PromoId);

            return View(appointment);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Edit(int id, Appointment appointment)
        {
            if (id != appointment.Id)
                return NotFound();

            if (!ModelState.IsValid)
            {
                ViewData["UserId"] = new SelectList(_context.Users, "Id", "FullName", appointment.UserId);
                ViewData["ServiceId"] = new SelectList(_context.Services, "Id", "Title", appointment.ServiceId);
                ViewData["PromoId"] = new SelectList(_context.Promos, "Id", "Title", appointment.PromoId);
                return View(appointment);
            }

            try
            {
                appointment.UpdatedAt = DateTime.Now;
                appointment.LastUpdatedAt = DateTime.Now;
                appointment.UpdatedBy = appointment.UserId!;
                appointment.LastUpdatedBy = appointment.UserId!;
                _context.Update(appointment);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AppointmentExists(appointment.Id))
                    return NotFound();
                else
                    throw;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Appointments/Delete/5
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
                return NotFound();

            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.CustomerService)
                .Include(a => a.Promo)
                .FirstOrDefaultAsync(m => m.Id == id);

            return appointment == null ? NotFound() : View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                _context.Appointments.Remove(appointment);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool AppointmentExists(int id)
        {
            return _context.Appointments.Any(e => e.Id == id);
        }
    }
}
