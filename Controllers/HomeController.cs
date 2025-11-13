using System.Diagnostics;
using New_LeRayBookingSystem.Models;
using Microsoft.AspNetCore.Mvc;
using New_LeRayBookingSystem.Data;
using New_LeRayBookingSystem.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity; // Required for IdentityConstants
using System.Collections.Generic;
using System.Threading.Tasks;

namespace New_LeRayBookingSystem.Controllers
{
    // Using primary constructor for cleaner dependency injection
    public class HomeController(
        ILogger<HomeController> logger, 
        ApplicationDbContext context) : Controller
    {
        private readonly ILogger<HomeController> _logger = logger;
        private readonly ApplicationDbContext _context = context;

        // Updated Index to pass models to the view
        public async Task<IActionResult> Index()
        {
            var model = new HomeIndexViewModel
            {
                Services = await _context.Services.ToListAsync(),
                Promos = await _context.Promos.ToListAsync()
            };
            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult ClientHistory()
        {
        return View();
        }
        
        [AllowAnonymous] 
        public IActionResult About()
        {
            return View(); 
        }

        [AllowAnonymous] 
        public IActionResult ContactUs()
        {
            return View(); 
        }

        // FIX: Enforce Cookie Scheme for protected MVC View access to resolve 401 error
       [Authorize(AuthenticationSchemes = "Identity.Application")]
        public IActionResult Booking()
        {
            return View(); 
        }
        [AllowAnonymous]
        public IActionResult TermsAndPrivacy()
        {
            return View();
        }

        // --- Services Actions ---

        [AllowAnonymous]
        public IActionResult Facial()
        {
            return View("Services/Facial"); 
        }

        [AllowAnonymous]
        public IActionResult LaserTreatment()
        {
            return View("Services/LaserTreatment");
        }

        [AllowAnonymous]
        public IActionResult SkinTreatment()
        {
            return View("Services/SkinTreatment");
        }

        [AllowAnonymous]
        public IActionResult Spa()
        {
            return View("Services/Spa");
        }

        [AllowAnonymous]
        public IActionResult Slimming()
        {
            return View("Services/Slimming");
        }

        [AllowAnonymous]
        public IActionResult LaserHairRemoval()
        {
            return View("Services/LaserHairRemoval");
        }

        [AllowAnonymous]
        public IActionResult Eyelash()
        {
            return View("Services/Eyelash");
        }

        [AllowAnonymous]
        public IActionResult MicroNeedling()
        {
            return View("Services/MicroNeedling");
        }

        [AllowAnonymous]
        public IActionResult NailCare()
        {
            return View("Services/NailCare");
        }

        [AllowAnonymous]
        public IActionResult SemiPermanentMakeup()
        {
            return View("Services/SemiPermanentMakeup");
        }

        [AllowAnonymous]
        public IActionResult Whitening()
        {
            return View("Services/Whitening");
        }

        [AllowAnonymous]
        public IActionResult ViewAllServices()
        {
            return View(); 
        }

        // --- Error Handling ---

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel 
            { 
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
            });
        }
    }
    
    // Moved HomeIndexViewModel outside the controller class
    public class HomeIndexViewModel
    {
        public required List<Service> Services { get; set; }
        public required List<Promos> Promos { get; set; }
    }
}