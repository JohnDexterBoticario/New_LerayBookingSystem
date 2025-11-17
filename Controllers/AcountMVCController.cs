using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using New_LeRayBookingSystem.Models;
using New_LeRayBookingSystem.Models.DTOs;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using New_LeRayBookingSystem.Services; // <-- ADDED

namespace New_LeRayBookingSystem.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService; // <-- ADDED

        public AccountController(UserManager<ApplicationUser> userManager, IEmailService emailService) // <-- MODIFIED CONSTRUCTOR
        {
            _userManager = userManager;
            _emailService = emailService; // <-- ADDED ASSIGNMENT
        }

        // GET: /Account/ForgotPassword
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            
            // Security: Always show confirmation even if user is null
            if (user != null)
            {
                // Generate password reset token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

                // Build reset link
                var resetLink = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { email = user.Email, token = encodedToken },
                    protocol: Request.Scheme
                );

                // START: SEND EMAIL IMPLEMENTATION
                var emailBody = $@"
                    Hello {user.FullName},<br/><br/>
                    You requested a password reset. Please click the link below to reset your password:<br/><br/>
                    <a href='{resetLink}'>Reset Password Link</a><br/><br/>
                    If you did not request this, please ignore this email.
                ";

                try
                {
                    await _emailService.SendEmailAsync(user.Email!, "Password Reset Request", emailBody);
                }
                catch (Exception ex)
                {
                    // Log the failure but continue to the confirmation screen for security best practice (not revealing if the user exists).
                    Console.WriteLine($@"PASSWORD RESET EMAIL FAILURE for {user.Email}: {ex.Message}");
                }
                // END: SEND EMAIL IMPLEMENTATION
            }

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        // GET: /Account/ForgotPasswordConfirmation
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/ResetPassword
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Error", "Home");
            }

            var model = new ResetPasswordDto
            {
                Email = email,
                Token = token
            };

            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Security: do not reveal user existence
                return RedirectToAction("ResetPasswordConfirmation");
            }

            var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            return RedirectToAction("ResetPasswordConfirmation");
        }

        // GET: /Account/ResetPasswordConfirmation
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}