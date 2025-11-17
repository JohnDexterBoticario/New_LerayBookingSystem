using New_LeRayBookingSystem.Models.DTOs;
namespace New_LeRayBookingSystem.Services
{
    public interface IAuthService
    {
        Task<AuthResultDto> RegisterAsync(RegisterDto registerDto);
        Task<AuthResultDto> LoginAsync(LoginDto loginDto);
        Task<AuthResultDto> VerifyOtpAsync(OtpVerifyDto verifyDto);
        Task<AuthResultDto> VerifyMfaAsync(OtpVerifyDto verifyDto);
        Task<AuthResultDto> SendOtpAsync(string email); // added to interface
        // Add SocialLoginAsync if needed
    }
}
