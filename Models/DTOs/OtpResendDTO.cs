
using System.ComponentModel.DataAnnotations;

namespace New_LeRayBookingSystem.Models.DTOs
{
    public class OtpResendDto
    {
        [Required]
        public required string Email { get; set; }
    }
}
