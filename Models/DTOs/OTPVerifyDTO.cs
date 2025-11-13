using System.ComponentModel.DataAnnotations;

namespace New_LeRayBookingSystem.Models.DTOs
{
    public class OtpVerifyDto
    {
        [Required]
        public required string Identifier { get; set; } // Email or Phone
        [Required]
        public required string Otp { get; set; }
    }
}