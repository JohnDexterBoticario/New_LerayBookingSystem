using New_LeRayBookingSystem.Services;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace New_LeRayBookingSystem.Controllers
{
    public class EmailTestController : Controller
    {
        private readonly IEmailService _emailService;

        public EmailTestController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> SendTest()
        {
            try
            {
                await _emailService.SendEmailAsync(
                    "omegapink24@gmail.com",  // 👈 replace with your email
                    "Test Email from LeRayBookingSystem",
                    "<h2>This is a MailKit test email ✅</h2><p>If you see this, your SMTP is working!</p>"
                );

                return Content("✅ Test email sent successfully. Check your inbox.");
            }
            catch (Exception ex)
            {
                return Content($"❌ Failed to send email: {ex.Message}");
            }
        }
    }
}
