
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using New_LeRayBookingSystem.Models;
using New_LeRayBookingSystem.Models.DTOs;
using New_LeRayBookingSystem.Services;

namespace New_LeRayBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IEmailService _emailService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager,
            IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _emailService = emailService;
        }

        // ====================================================================
        // REGISTER (SIGNUP)
        // ====================================================================
        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid data.",
                    errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)
                });

            // Basic name validation (letters, spaces, hyphen, apostrophe)
            var nameRegex = new System.Text.RegularExpressions.Regex(@"^[A-Za-z\s'-]+$");
            if (!nameRegex.IsMatch(model.FirstName) || !nameRegex.IsMatch(model.LastName))
                return BadRequest(new { success = false, message = "First and Last name must contain letters only." });

            // Prevent duplicate email
            var existingEmail = await _userManager.FindByEmailAsync(model.Email);
            if (existingEmail != null)
                return Conflict(new { success = false, message = "Email already registered." });

            try
            {
                var user = new ApplicationUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    FullName = $"{model.FirstName ?? "User"} {model.LastName ?? ""}".Trim(),
                    DateJoined = DateTime.UtcNow,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = false
                };

                var createResult = await _userManager.CreateAsync(user, model.Password);
                if (!createResult.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Registration failed.",
                        errors = createResult.Errors.Select(e => e.Description)
                    });
                }

                // Ensure role 'Client' exists and assign
                if (!await _roleManager.RoleExistsAsync("Client"))
                    await _roleManager.CreateAsync(new IdentityRole("Client"));

                await _userManager.AddToRoleAsync(user, "Client");

                // Generate OTP and send email
                var otp = new Random().Next(100000, 999999).ToString();
                user.TwoFactorCode = otp;
                await _userManager.UpdateAsync(user);

                var emailBody = $@"
                    Hello {user.FullName},<br/><br/>
                    Your OTP verification code is: <strong>{otp}</strong><br/><br/>
                    Please enter this code to verify your account.
                ";

                try
                {
                    await _emailService.SendEmailAsync(user.Email!, "Email Verification Code", emailBody);
                }
                catch (Exception ex)
                {
                    // Log and continue - user is created
                    Console.WriteLine($"OTP SEND FAILURE: {ex.Message}");
                    // Inform client that sending failed but account created
                    return Ok(new { success = true, message = "Registration successful. OTP generation succeeded but email sending failed. Contact admin or check logs.", email = user.Email });
                }

                return Ok(new { success = true, message = "Registration successful. OTP sent to email.", email = user.Email });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL REGISTRATION ERROR: {ex.Message}");
                return StatusCode(500, new { success = false, message = "An internal server error occurred. Check logs." });
            }
        }

        // ====================================================================
        // LOGIN
        // ====================================================================
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            if (string.IsNullOrWhiteSpace(model.EmailOrUsername) || string.IsNullOrWhiteSpace(model.Password))
                return BadRequest(new { message = "Email/Username and Password are required." });

            string loginValue = model.EmailOrUsername.Trim();
            ApplicationUser? user = null;

            // If value looks like email, use FindByEmail; otherwise treat as username
            if (System.Text.RegularExpressions.Regex.IsMatch(loginValue, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                user = await _userManager.FindByEmailAsync(loginValue);

            if (user == null)
                user = await _userManager.FindByNameAsync(loginValue);

            if (user == null)
                return Unauthorized(new { message = "Invalid credentials." });

            if (!user.EmailConfirmed)
                return Unauthorized(new { message = "Email not verified. Please verify before login." });

            var signInResult = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
            if (!signInResult.Succeeded)
            {
                if (signInResult.IsLockedOut)
                    return Unauthorized(new { message = "Account locked out." });
                if (signInResult.IsNotAllowed)
                    return Unauthorized(new { message = "Login not allowed." });

                return Unauthorized(new { message = "Invalid credentials." });
            }

            var roles = await _userManager.GetRolesAsync(user);
            string redirectUrl = roles.Contains("SuperAdmin") || roles.Contains("Admin") ? "/Admin/Dashboard" : "/Home/Booking";

            return Ok(new
            {
                message = "Login successful.",
                redirectUrl,
                user = new { email = user.Email, name = user.FullName, roles }
            });
        }

        [HttpPost("logout")]
        [Authorize] // Requires the user to be signed in to log out
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { success = true, message = "Logout successful." });
        }

        // ====================================================================
        // VERIFY OTP
        // ====================================================================
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data." });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest(new { success = false, message = "User not found." });

            if (user.TwoFactorCode != model.Code)
                return BadRequest(new { success = false, message = "Invalid OTP code." });

            user.EmailConfirmed = true;
            user.TwoFactorCode = null;
            await _userManager.UpdateAsync(user);

            return Ok(new { success = true, message = "OTP verified successfully." });
        }

        // ====================================================================
        // RESEND OTP
        // ====================================================================
        [HttpPost("resend-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendOtp([FromBody] OtpResendDto model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { success = false, message = "Invalid data." });

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return BadRequest(new { success = false, message = "User not found." });

            var otp = new Random().Next(100000, 999999).ToString();
            user.TwoFactorCode = otp;
            await _userManager.UpdateAsync(user);

            var emailBody = $@"
                Hello {user.FullName},<br/><br/>
                Your new OTP verification code is: <strong>{otp}</strong><br/><br/>
                Please enter this code to verify your account.
            ";

            try
            {
                await _emailService.SendEmailAsync(user.Email!, "Email Verification Code", emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OTP RESEND FAILURE: {ex.Message}");
                return Ok(new { success = true, message = "OTP generated but failed to send email. Check logs." });
            }

            return Ok(new { success = true, message = "OTP resent successfully." });
        }
    }
}
