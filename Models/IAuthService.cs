using New_LeRayBookingSystem.Models.DTOs;
 
namespace New_LeRayBookingSystem.Models
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
        Task<AuthResultDto> VerifyOtpAsync(OtpVerifyDto verifyDto);
        Task<AuthResultDto> VerifyMfaAsync(OtpVerifyDto verifyDto);
        // Add SocialLoginAsync if needed
    }
}