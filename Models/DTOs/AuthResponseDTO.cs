
namespace New_LeRayBookingSystem.Models.DTOs  
{
    public class AuthResponseDto
    {
        public bool IsSuccess { get; set; }
        public required string Message { get; set; }
        public string? Token { get; set; }
        public UserDto? User { get; set; } // The red line will now disappear
        public bool MfaRequired { get; set; } = false;
    }
}