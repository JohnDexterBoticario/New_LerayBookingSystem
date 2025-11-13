using System.ComponentModel.DataAnnotations;

namespace New_LeRayBookingSystem.Models.DTOs
{
    public class ForgotPasswordDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}