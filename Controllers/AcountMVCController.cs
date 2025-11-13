using Microsoft.AspNetCore.Mvc;
using New_LeRayBookingSystem.Models.DTOs; // Assuming DTOs are here

namespace New_LeRayBookingSystem.Controllers
{
    // This MVC controller serves Razor views, it is distinct from AccountApiController
    public class AccountController : Controller
    {
        // GET: /Account/ForgotPassword
        // This serves the initial form to request a password reset email.
        public IActionResult ForgotPassword()
        {
            // You can pass an empty model if needed, but for a simple view, null is fine.
            return View(); 
        }

        // GET: /Account/ForgotPasswordConfirmation
        // This is the confirmation page shown after the email has been 'sent'.
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        // GET: /Account/ResetPassword?email=...&token=...
        // This serves the form where the user sets their new password, using the token from the email link.
        public IActionResult ResetPassword(string email, string token)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(token))
            {
                // Handle missing parameters or redirect to an error page
                return RedirectToAction("Error", "Home"); 
            }

            // Pass the email and token to the view via the ViewModel or ViewData/TempData
            var model = new ResetPasswordDto { Email = email, Token = token };

            return View(model);
        }

        // GET: /Account/ResetPasswordConfirmation
        // This is the success page shown after the password has been successfully reset.
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }
    }
}