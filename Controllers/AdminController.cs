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

namespace New_LeRayBookingSystem.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            // --- Load related data (Appointments + Service + User)
            var appointments = await _context.Appointments
                .Include(a => a.CustomerService)
                .Include(a => a.User)
                .ToListAsync();

            // --- Overall counts
            var totalAppointments = appointments.Count;
            var pendingAppointments = appointments.Count(a => a.Status == "Pending");
            var approvedAppointments = appointments.Count(a => a.Status == "Approved");
            var cancelledAppointments = appointments.Count(a => a.Status == "Cancelled");

            // --- Sales / Revenue (for Approved only)
            var totalSales = appointments
                .Where(a => a.Status == "Approved")
                .Sum(a => (decimal?)a.CustomerService?.Price ?? 0m);

            // --- Appointments by month (for chart)
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

            // --- Sales over time (monthly revenue)
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

            // --- Top 5 services
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

            // --- Upcoming appointments (next 5)
            var upcomingAppointments = appointments
                .Where(a => a.AppointmentDate >= DateTime.Now)
                .OrderBy(a => a.AppointmentDate)
                .Take(5)
                .Select(a => new UpcomingAppointment
                {
                    ClientName = a.User?.FullName ?? "Unknown",
                    ServiceName = a.CustomerService?.ServiceName ?? a.ServiceName ?? "Unknown",
                    Date = a.AppointmentDate
                })
                .ToList();

            // --- Recent appointments (latest 5)
            var recentAppointments = appointments
                .OrderByDescending(a => a.AppointmentDate)
                .Take(5)
                .Select(a => new RecentAppointment
                {
                    ClientName = a.User?.FullName ?? "Unknown",
                    ServiceName = a.CustomerService?.ServiceName ?? a.ServiceName ?? "Unknown",
                    Date = a.AppointmentDate,
                    Status = a.Status
                })
                .ToList();

            // --- Build final ViewModel
            var viewModel = new DashboardViewModel
            {
                TotalAppointments = totalAppointments,
                PendingAppointments = pendingAppointments,
                ApprovedAppointments = approvedAppointments,
                CancelledAppointments = cancelledAppointments,
                TotalSales = totalSales ,
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
