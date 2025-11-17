using New_LeRayBookingSystem.Models.DTOs;
using New_LeRayBookingSystem.Models;
using New_LeRayBookingSystem.Services; // Required for IEmailService
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities; 
using System.Text; 
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc.Routing; 
using Microsoft.EntityFrameworkCore; // Required for FirstOrDefaultAsync

namespace New_LerayBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountApiController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        IUrlHelper urlHelper,
        IEmailService emailService) : ControllerBase // <-- ADDED IEmailService
    {
        private readonly UserManager<ApplicationUser> _userManager = userManager;
        private readonly SignInManager<ApplicationUser> _signInManager = signInManager;
        private readonly RoleManager<IdentityRole> _roleManager = roleManager;
        private readonly IUrlHelper _urlHelper = urlHelper; 
        private readonly IEmailService _emailService = emailService; // <-- ADDED

        // ===========================================================
        // 🟢 REGISTER (Signup)
        // ===========================================================
        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // ✅ Name validation (backend check)
            var nameRegex = new Regex(@"^[A-Za-z\s'-]+$");
            if (!nameRegex.IsMatch(model.FirstName) || !nameRegex.IsMatch(model.LastName))
            {
                return BadRequest(new { message = "First name and Last name must contain letters only." });
            }

            // ✅ Prevent duplicate email
            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
                return Conflict(new { message = "Email already registered." });

            // ✅ Create new ApplicationUser
            var user = new ApplicationUser
            {
                UserName = model.Email, // Identity uses UserName as the primary login ID (can be email)
                Email = model.Email,
                FullName = $"{model.FirstName} {model.LastName}",
                DateJoined = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(new
                {
                    message = "Registration failed.",
                    errors = result.Errors.Select(e => e.Description)
                });

            // ✅ Ensure "Client" role exists
            if (!await _roleManager.RoleExistsAsync("Client"))
                await _roleManager.CreateAsync(new IdentityRole("Client"));

            await _userManager.AddToRoleAsync(user, "Client");

            return Ok(new { message = "Registration successful." });
        }

        // ===========================================================
        // 🟢 LOGIN (Email / Username / Phone)
        // ===========================================================
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (string.IsNullOrWhiteSpace(model.EmailOrUsername) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { message = "Email/Username/Phone and Password are required." });

            string loginValue = model.EmailOrUsername.Trim();
            ApplicationUser? user = null;

            // 1. Try finding by Email
            if (Regex.IsMatch(loginValue, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            {
                user = await _userManager.FindByEmailAsync(loginValue);
            }
            
            // 2. If not found, try finding by Phone Number
            if (user == null && Regex.IsMatch(loginValue, @"^\+?\d{10,15}$")) 
            {
                user = await _userManager.Users.FirstOrDefaultAsync(u => u.PhoneNumber == loginValue);
            }

            // 3. If still not found, try finding by Username (since Username is set to Email during signup, this acts as a final email/username check)
            if (user == null)
            {
                 user = await _userManager.FindByNameAsync(loginValue);
            }

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials." });

            // FIX: Removed redundant SignOutAsync as PasswordSignInAsync handles sign-in if successful.
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
            
            if (!result.Succeeded)
            {
                // Provide more specific error feedback if needed
                if (result.IsLockedOut)
                    return Unauthorized(new { message = "Account locked out. Please try again later." });
                if (result.IsNotAllowed)
                    return Unauthorized(new { message = "Login not allowed (e.g., email not confirmed)." });

                return Unauthorized(new { message = "Invalid credentials." });
            }

            // FIX: Removed redundant SignInAsync(user, isPersistent: false);

            var roles = await _userManager.GetRolesAsync(user);
            
            // Determine redirect URL based on roles (matching logic in Program.cs)
            string redirectUrl = roles.Contains("SuperAdmin") ? "/Admin/Dashboard" :
                                 roles.Contains("Admin") ? "/Admin/Dashboard" :
                                 "/Home/Booking"; // Default Client path

            return Ok(new
            {
                message = "Login successful.",
                redirectUrl = redirectUrl,
                user = new
                {
                    email = user.Email,
                    name = user.FullName,
                    roles
                }
            });
        }

        // ===========================================================
        // 🟠 LOGOUT
        // ===========================================================
        [Authorize(AuthenticationSchemes = "Identity.Application")]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();

    // Detect if request is API (JSON) or Web (HTML)
    if (Request.Headers["Accept"].ToString().Contains("application/json"))
    {
        // Return JSON for API logout
        return Ok(new { message = "Logged out successfully." });
    }

    // Otherwise redirect to Home page (for web UI logout)
    return RedirectToAction("Index", "Home");
        }

[Authorize]
[HttpGet]
public async Task<IActionResult> LogoutGet()
{
    await _signInManager.SignOutAsync();

    // Same logic for GET requests
    if (Request.Headers["Accept"].ToString().Contains("application/json"))
    {
        return Ok(new { message = "Logged out successfully." });
    }

    return RedirectToAction("Index", "Home");
}

        // ===========================================================
        // 🟡 FORGOT PASSWORD (Request Reset Link)
        // ===========================================================
        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);

            // IMPORTANT SECURITY STEP: Always return OK, regardless of whether the email exists.
            if (user == null)
            {
                return Ok(new { message = "If an account exists for this email, a password reset link has been sent." });
            }

            // 1. Generate the password reset token
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // 2. Build the password reset URL
            var resetLink = _urlHelper.Action(
                action: "ResetPassword",
                controller: "Account", 
                values: new { email = user.Email, token = encodedToken },
                protocol: Request.Scheme
            );

            // 3. FIX: Send the email with the resetLink using IEmailService.
            var emailBody = $"Please reset your password by clicking the link below:<br/><br/><a href='{resetLink}' style='color:#4F46E5;'>Reset Password Link</a><br/><br/>If you did not request a password reset, please ignore this email.";
            
            await _emailService.SendEmailAsync(
                toEmail: user.Email!, // Email is guaranteed to exist here
                subject: "Password Reset Request", 
                messageHtml: emailBody); 

            // Return success (only include resetLink for debugging/testing environments)
            return Ok(new 
            { 
                message = "If an account exists for this email, a password reset link has been sent.", 
                resetLink = resetLink 
            });
        }


        // ===========================================================
        // 🟣 RESET PASSWORD (Set New Password)
        // ===========================================================
        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Return a generic error message for security.
                return BadRequest(new { message = "Password reset failed due to an invalid request." });
            }

            // Decode the token from the URL-safe format
            var decodedTokenBytes = WebEncoders.Base64UrlDecode(model.Token);
            var decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);

            // Use the built-in Identity method to change the password
            var result = await _userManager.ResetPasswordAsync(user, decodedToken, model.NewPassword);

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Password reset failed.",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            // Optional: Sign out the user from all sessions for security
            // await _userManager.UpdateSecurityStampAsync(user);

            return Ok(new { message = "Password has been reset successfully. You can now log in with your new password." });
        }
    }
}