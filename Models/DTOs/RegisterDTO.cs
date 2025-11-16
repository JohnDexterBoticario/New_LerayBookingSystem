using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace New_LeRayBookingSystem.Models.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "First name is required.")]
        [RegularExpression(@"^[A-Za-z\s'-]+$", ErrorMessage = "First name must contain letters only.")]
        public required string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        [RegularExpression(@"^[A-Za-z\s'-]+$", ErrorMessage = "Last name must contain letters only.")]
        public required string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters long.")]
        public required string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required.")]
        [Compare("Password", ErrorMessage = "Passwords do not match.")]
        public required string ConfirmPassword { get; set; }
    }
}
