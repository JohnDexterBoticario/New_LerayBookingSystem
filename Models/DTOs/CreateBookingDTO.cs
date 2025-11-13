using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using System.Globalization;
using New_LeRayBookingSystem.ValidationAttributes;
using System; 

namespace New_LeRayBookingSystem.Models.DTOs
{
    public class CreateBookingDto
    {
        // FIX 1: Removed FullName and Email properties. 
        // These are fetched from the authenticated user in the controller (BookingsController) 
        // and should not be submitted by the client in a secure flow.
        
        [Required(ErrorMessage = "A service ID is required for booking.")]
        public string ServiceId { get; set; } = string.Empty;

        public decimal TotalPrice { get; set; }

        [Required(ErrorMessage = "Please select an appointment date.")]
        public string AppointmentDateString { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select an available time slot.")]
        public string TimeSlotString { get; set; } = string.Empty;

        // FIX 2: Added the PaymentMethod property, required by the Appointment model
        [Required(ErrorMessage = "A payment method must be specified.")]
        [MaxLength(50)]
        public string PaymentMethod { get; set; } = string.Empty;

        public string? Notes { get; set; }

       [Required(ErrorMessage = "Please upload a clear payment receipt.")]
// Add the checks based on the image's requirements:
       [MaxFileSize(25 * 1024 * 1024, ErrorMessage = "File size exceeds 25MB limit.")]
       [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png" }, ErrorMessage = "Only JPG, JPEG, and PNG files are allowed.")]
public required IFormFile PaymentReceiptFile { get; set; }

        // --- Computed Property ---
        public DateTime StartTime
        {
            get
            {
                if (string.IsNullOrEmpty(AppointmentDateString) || string.IsNullOrEmpty(TimeSlotString))
                {
                    return DateTime.MinValue;
                }
                
                // Define the date formats we expect. It's common to use multiple formats for robustness.
                var dateFormats = new[] { 
                    "M/d/yyyy",     // e.g., 1/1/2025
                    "MM/dd/yyyy"    // e.g., 01/01/2025
                };

                // 1. Parse the Date part
                if (!DateTime.TryParseExact(
                    AppointmentDateString,
                    dateFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime datePart))
                {
                    // If date parsing fails, return MinValue
                    return DateTime.MinValue;
                }

                // 2. Extract and Parse the Time part
                var startTimeString = TimeSlotString.Split('-')[0].Trim();

                // Define the time formats we expect (h:mm tt for 9:00 AM, hh:mm tt for 09:00 AM)
                var timeFormats = new[] { "h:mm tt", "hh:mm tt", "H:mm", "HH:mm" }; 
                
                if (DateTime.TryParseExact(
                    startTimeString, 
                    timeFormats, 
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.NoCurrentDateDefault, 
                    out DateTime timePart))
                {
                    // Combine the parsed Date (datePart.Date) with the parsed Time (timePart.TimeOfDay)
                    // The returned DateTime is in the client's local time zone (unspecified kind).
                    return datePart.Date.Add(timePart.TimeOfDay);
                }

                return DateTime.MinValue;
            }
        }
    }
}