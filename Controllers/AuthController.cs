using Microsoft.AspNetCore.Mvc;
using New_LeRayBookingSystem.Models.DTOs;
using New_LeRayBookingSystem.Services; // Assuming IAuthService lives here
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace New_LeRayBookingSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(IAuthService authService) : ControllerBase
    {
        private readonly IAuthService _authService = authService;

        // --- AUTHENTICATION ENDPOINTS ---

        [HttpPost("register")]
        [AllowAnonymous] // 💡 FIX 1: Explicitly allow unauthenticated access
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            if (registerDto == null)
                return BadRequest("Request body cannot be null.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.RegisterAsync(registerDto);
            
            if (!result.IsSuccess)
                // Return 400 Bad Request for registration errors (e.g., user exists, bad password format)
                return BadRequest(new { result.Message }); 

            // Assuming result contains a confirmation message or necessary data
            return Ok(result); 
        }

        [HttpPost("login")]
        [AllowAnonymous] // 💡 FIX 1: Explicitly allow unauthenticated access
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.LoginAsync(loginDto);
            
            if (!result.IsSuccess)
                // 💡 FIX 2: Use 401 Unauthorized for failed logins (wrong credentials)
                return Unauthorized(new { result.Message }); 

            // Assuming result.Data contains the JWT token and user info needed by the frontend
            return Ok(result.Data); 
        }
        
        // --- VERIFICATION ENDPOINTS ---

        [HttpPost("verify-otp")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpVerifyDto verifyDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.VerifyOtpAsync(verifyDto);
            if (!result.IsSuccess)
                return BadRequest(new { result.Message });

            // Assuming result.Data contains the JWT token for automatic login after OTP success
            return Ok(result.Data); 
        }

        [HttpPost("verify-mfa")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyMfa([FromBody] OtpVerifyDto verifyDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.VerifyMfaAsync(verifyDto);
            if (!result.IsSuccess)
                return BadRequest(new { result.Message });

            // Assuming result.Data contains the JWT token after successful MFA
            return Ok(result.Data);
        }

        // --- SOCIAL LOGIN (Optional) ---
        
        [HttpPost("social-login")]
        [AllowAnonymous]
        public IActionResult SocialLogin([FromBody] SocialLoginDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid social login data.");

            // 💡 FIX 3: Ensure SocialLoginDto is now defined outside the controller 
            // (or imported from your DTOs folder)

            // Implement actual authentication logic here
            // return _authService.SocialLoginAsync(dto);

            return Ok(new { Message = "Social login endpoint is working (Requires implementation)." });
        }
    }

    // This class should be moved to its own file in the DTOs folder
    public class SocialLoginDto
    {
        public string Provider { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}