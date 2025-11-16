using Microsoft.AspNetCore.Mvc;
using New_LeRayBookingSystem.Models.DTOs;
using New_LeRayBookingSystem.Models;
using New_LeRayBookingSystem.Services;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace New_LeRayBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService, IAuditService auditService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;
        private readonly IAuditService _auditService = auditService;

        // =============================
        //           REGISTER
        // =============================
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
                return BadRequest("Request body cannot be null.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(registerDto);

            if (!result.IsSuccess)
            {
                await _auditService.LogAsync(
                    action: "RegisterFailed",
                    entityType: "User",
                    entityId: registerDto.Email,
                    details: $"Registration failed: {result.Message}",
                    module: "Auth",
                    description: $"Registration failed for email {registerDto.Email}"
                );

                return BadRequest(new { result.Message });
            }

            await _auditService.LogAsync(
                action: "Register",
                entityType: "User",
                entityId: registerDto.Email,
                details: "User registered successfully",
                module: "Auth",
                description: $"User registered successfully with email {registerDto.Email}"
            );

            return Ok(result);
        }

        // =============================
        //            LOGIN
        // =============================
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);

            if (!result.IsSuccess)
            {
                await _auditService.LogAsync(
                    action: "LoginFailed",
                    entityType: "User",
                    entityId: loginDto.EmailOrUsername,  // Changed from Email
                    details: $"Failed login attempt",
                    module: "Auth",
                    description: $"Failed login attempt for {loginDto.EmailOrUsername}"  // Changed from Email
                );

                return Unauthorized(new { result.Message });
            }

            await _auditService.LogAsync(
                action: "Login",
                entityType: "User",
                entityId: loginDto.EmailOrUsername,  // Changed from Email
                details: "User logged in successfully",
                module: "Auth",
                description: $"User '{loginDto.EmailOrUsername}' logged in successfully."  // Changed from Email
            );

            return Ok(result.Data);
        }

        // =============================
        //          VERIFY OTP
        // =============================
        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyDto verifyDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.VerifyOtpAsync(verifyDto);

            if (!result.IsSuccess)
            {
                await _auditService.LogAsync(
                    action: "OtpFailed",
                    entityType: "User",
                    entityId: verifyDto.Identifier,  // Changed from Email
                    details: "OTP verification failed",
                    module: "Auth",
                    description: $"OTP verification failed for user {verifyDto.Identifier}"  // Changed from Email
                );

                return BadRequest(new { result.Message });
            }

            await _auditService.LogAsync(
                action: "OtpVerified",
                entityType: "User",
                entityId: verifyDto.Identifier,  // Changed from Email
                details: "OTP verified successfully",
                module: "Auth",
                description: $"OTP verified successfully for user {verifyDto.Identifier}"  // Changed from Email
            );

            return Ok(result.Data);
        }

        // =============================
        //          VERIFY MFA
        // =============================
        [HttpPost("verify-mfa")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyMfa([FromBody] OtpVerifyDto verifyDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.VerifyMfaAsync(verifyDto);

            if (!result.IsSuccess)
            {
                await _auditService.LogAsync(
                    action: "MfaFailed",
                    entityType: "User",
                    entityId: verifyDto.Identifier,  // Changed from Email
                    details: "MFA verification failed",
                    module: "Auth",
                    description: $"MFA verification failed for user {verifyDto.Identifier}"  // Changed from Email
                );

                return BadRequest(new { result.Message });
            }

            await _auditService.LogAsync(
                action: "MfaVerified",
                entityType: "User",
                entityId: verifyDto.Identifier,  // Changed from Email
                details: "MFA verified successfully",
                module: "Auth",
                description: $"MFA verified successfully for user {verifyDto.Identifier}"  // Changed from Email
            );

            return Ok(result.Data);
        }

        // =============================
        //        SOCIAL LOGIN
        // =============================
        [HttpPost("social-login")]
        [AllowAnonymous]
        public async Task<IActionResult> SocialLogin([FromBody] SocialLoginDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid social login data.");

            await _auditService.LogAsync(
                action: "SocialLogin",
                entityType: "User",
                entityId: "Unknown",
                details: $"Social login using provider {dto.Provider}",
                module: "Auth",
                description: $"Social login attempt using provider {dto.Provider}"
            );

            return Ok(new { Message = "Social login endpoint is working (Requires implementation)." });
        }
    }

    // Move to /Models/DTOs/SocialLoginDto.cs
    public class SocialLoginDto
    {
        public string Provider { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}