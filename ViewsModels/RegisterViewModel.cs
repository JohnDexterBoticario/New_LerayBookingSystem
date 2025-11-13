using System.ComponentModel.DataAnnotations;

namespace New_LeRayBookingSystem.ViewModels
{
    public class RegisterViewModel
    {
        // Must match the ApplicationUser properties
        [Required]
        public string FirstName { get; set; } = string.Empty;
        public string LastName{ get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        // Note: The C# Password field should not be named 'passwordHash' for a registration DTO
        [Required, DataType(DataType.Password)] 
        public string Password { get; set; } = string.Empty; 
    }
}