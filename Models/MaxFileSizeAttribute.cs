using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace New_LeRayBookingSystem.ValidationAttributes
{
    // The constructor takes the maximum allowed size in bytes
    public class MaxFileSizeAttribute : ValidationAttribute
    {
        private readonly int _maxFileSize;

        public MaxFileSizeAttribute(int maxFileSize)
        {
            _maxFileSize = maxFileSize;
        }

        protected override ValidationResult? IsValid(
            object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                if (file.Length > _maxFileSize)
                {
                    // Use a default error message if none is provided
                    return new ValidationResult(
                        ErrorMessage ?? $"Maximum allowed file size is {(_maxFileSize / 1024 / 1024)} MB."
                    );
                }
            }
            // If value is null, the [Required] attribute handles it.
            return ValidationResult.Success;
        }
    }
}