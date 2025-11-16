using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using New_LeRayBookingSystem.Data;
using New_LeRayBookingSystem.Models;
using New_LeRayBookingSystem.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using New_LeRayBookingSystem.Services;

namespace New_LeRayBookingSystem.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _audit;

        public AdminController(ApplicationDbContext context, IAuditService audit)
        {
            _context = context;
            _audit = audit;
        }

        private string UserId() =>
            User?.Identity?.IsAuthenticated == true ? User.Identity.Name ?? "Unknown" : "Anonymous";

        private string IP() =>
            HttpContext.Connection.RemoteIpAddress?.ToString();

        private string UA() =>
            Request.Headers["User-Agent"].ToString();

        // ---------------------------------------------------------------------------------------
        // GET: Admin/Calendar
        // ---------------------------------------------------------------------------------------
        public async Task<IActionResult> Calendar()
        {
            await _audit.LogAsync(
                UserId(),
                "Viewed",
                "Admin",
                "Viewed Calendar page",
                IP(),
                UA()
            );

            return View();
        }

        // ---------------------------------------------------------------------------------------
        // GET: Admin/Dashboard
        // ---------------------------------------------------------------------------------------
        public async Task<IActionResult> Dashboard()
        {
            await _audit.LogAsync(
                UserId(),
                "Viewed",
                "Admin",
                "Viewed Dashboard",
                IP(),
                UA()
            );

            // Load data with includes
            var appointments = await _context.Appointments
                .Include(a => a.CustomerService)
                .Include(a => a.User)
                .ToListAsync();

            // Overall counts
            var totalAppointments = appointments.Count;
            var pendingAppointments = appointments.Count(a => a.Status == "Pending");
            var approvedAppointments = appointments.Count(a => a.Status == "Approved");
            var cancelledAppointments = appointments.Count(a => a.Status == "Cancelled");

            // Sales for approved appointments
            var totalSales = appointments
                .Where(a => a.Status == "Approved")
                .Sum(a => (decimal?)a.CustomerService?.Price ?? 0m);

            // Appointments by month (for chart)
            var appointmentsByMonth = appointments
                .Where(a => a.Status == "Approved")
                .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
                .Select(g => new AppointmentByMonth
                {
                    Month = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key.Month)} {g.Key.Year}",
                    Count = g.Count()
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Sales over time
            var salesOverTime = appointments
                .Where(a => a.Status == "Approved")
                .GroupBy(a => new { a.AppointmentDate.Year, a.AppointmentDate.Month })
                .Select(g => new SalesDataPoint
                {
                    Month = $"{CultureInfo.CurrentCulture.DateTimeFormat.GetAbbreviatedMonthName(g.Key.Month)} {g.Key.Year}",
                    Revenue = g.Sum(a => (decimal?)a.CustomerService?.Price ?? 0m)
                })
                .OrderBy(x => x.Month)
                .ToList();

            // Top 5 services
            var topServices = appointments
                .Where(a => a.Status == "Approved" && a.CustomerService != null)
                .GroupBy(a => a.CustomerService!.ServiceName)
                .Select(g => new ServicePerformance
                {
                    ServiceName = g.Key ?? "Unknown",
                    AppointmentCount = g.Count(),
                    Percentage = approvedAppointments > 0
                        ? Math.Round((double)g.Count() / approvedAppointments * 100, 1)
                        : 0
                })
                .OrderByDescending(x => x.AppointmentCount)
                .Take(5)
                .ToList();

            // Upcoming appointments
            var upcomingAppointments = appointments
                .Where(a => a.AppointmentDate >= DateTime.Now)
                .OrderBy(a => a.AppointmentDate)
                .Take(5)
                .Select(a => new UpcomingAppointment
                {
                    ClientName = a.User?.FullName ?? "Unknown",
                    ServiceName = a.CustomerService?.ServiceName ?? "Unknown",
                    Date = a.AppointmentDate
                })
                .ToList();

            // Recent appointments
            var recentAppointments = appointments
                .OrderByDescending(a => a.AppointmentDate)
                .Take(5)
                .Select(a => new RecentAppointment
                {
                    ClientName = a.User?.FullName ?? "Unknown",
                    ServiceName = a.CustomerService?.ServiceName ?? "Unknown",
                    Date = a.AppointmentDate,
                    Status = a.Status
                })
                .ToList();

            var viewModel = new DashboardViewModel
            {
                TotalAppointments = totalAppointments,
                PendingAppointments = pendingAppointments,
                ApprovedAppointments = approvedAppointments,
                CancelledAppointments = cancelledAppointments,
                TotalSales = totalSales,
                AppointmentsByMonth = appointmentsByMonth,
                SalesOverTime = salesOverTime,
                TopServices = topServices,
                UpcomingAppointments = upcomingAppointments,
                RecentAppointments = recentAppointments
            };

            return View(viewModel);
        }
    }
}
