using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace New_LeRayBookingSystem.Controllers
{
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CalendarController : Controller
    {
        // This loads the Admin Calendar view (Calendar.cshtml)
        public IActionResult Index()
        {
            // Explicitly point to /Views/Admin/Calendar.cshtml
            return View("~/Views/Admin/Calendar.cshtml");
        }
    }
}
