using System.ComponentModel.DataAnnotations;

namespace New_LeRayBookingSystem.Models.DTOs
{
    public class OtpVerifyDto
    {
        [Required]
        public required string Email { get; set; } // Email or Phone

        [Required]
        public required string Code { get; set; }
    }
}