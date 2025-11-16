using Microsoft.AspNetCore.Mvc;
using New_LeRayBookingSystem.Services;


namespace New_LeRayBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailTestController : ControllerBase
    {
        private readonly IEmailService _emailService;

        public EmailTestController(IEmailService emailService)
        {
            _emailService = emailService;
        }

        /// <summary>
        /// Send a test email to verify SMTP configuration.
        /// Usage: GET /api/emailtest/send?email=youremail@gmail.com
        /// </summary>
        [HttpGet("send")]
        public async Task<IActionResult> SendTest([FromQuery] string? email)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(email))
                    email = "omegapink24@gmail.com"; // default test receiver

                await _emailService.SendEmailAsync(
                    email,
                    "📧 Test Email from LeRayBookingSystem",
                    "<h2>MailKit SMTP Test Successful ✅</h2><p>If you received this, your email settings are working correctly.</p>"
                );

                return Ok(new
                {
                    success = true,
                    message = "Test email sent successfully! Check your inbox.",
                    sentTo = email
                });
            }
            catch (Exception ex)
            {
                // Log the actual error to console for debugging
                Console.WriteLine("SMTP TEST ERROR: " + ex.Message);

                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to send test email.",
                    error = ex.Message
                });
            }
        }
    }
}
