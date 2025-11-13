
namespace New_LeRayBookingSystem.Models.DTOs
{
    public class TokenResponseDto
    {
        public string Token { get; set; } = null!;
        public UserDto User { get; set; } = null!; // The red line will now disappear
    }
}
