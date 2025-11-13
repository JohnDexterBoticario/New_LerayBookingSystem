using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Linq;

namespace New_LeRayBookingSystem.ValidationAttributes
{
    public class AllowedExtensionsAttribute : ValidationAttribute
    {
        private readonly string[] _extensions;

        public AllowedExtensionsAttribute(string[] extensions)
        {
            _extensions = extensions;
        }

        protected override ValidationResult? IsValid(
            object? value, ValidationContext validationContext)
        {
            if (value is IFormFile file)
            {
                var extension = Path.GetExtension(file.FileName);
                if (!_extensions.Contains(extension.ToLower()))
                {
                    // Use a default error message if none is provided
                    return new ValidationResult(
                        ErrorMessage ?? $"The file extension is not allowed. Only {string.Join(", ", _extensions)} are permitted."
                    );
                }
            }
            // If value is null, the [Required] attribute handles it.
            return ValidationResult.Success;
        }
    }
}