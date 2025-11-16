using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace New_LeRayBookingSystem.Models
{
    /// <summary>
    /// Represents an application user that extends the default IdentityUser
    /// with additional profile, audit, and relationship data.
    /// </summary>
    public class ApplicationUser : IdentityUser
    {
        // -------------------------
        // 🔹 Profile Fields
        // -------------------------
        [Required, MaxLength(100)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        // -------------------------
        // 🔹 Audit & Activity Fields
        // -------------------------
        [Display(Name = "Date Joined")]
        public DateTime DateJoined { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Login")]
        public DateTime LastLogin { get; set; } = DateTime.UtcNow;

        [Display(Name = "Created At")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Display(Name = "Updated At")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // -------------------------
        // 🔹 Navigation Properties
        // -------------------------
        public ICollection<Appointment>? Appointments { get; set; }

        [InverseProperty(nameof(Appointment.CreatedByUser))]
        public ICollection<Appointment>? CreatedAppointments { get; set; }

        [InverseProperty(nameof(Appointment.UpdatedByUser))]
        public ICollection<Appointment>? UpdatedAppointments { get; set; }

        [InverseProperty(nameof(Appointment.LastUpdatedByUser))]
        public ICollection<Appointment>? LastUpdatedAppointments { get; set; }


        // 🔹 OTP / Verification Fields
        public string? TwoFactorCode { get; set; }
        public DateTime? TwoFactorCodeExpiry { get; set; }


        // 🔹 Optional Loyalty Reference
        public Loyalty? Loyalty { get; set; }

        // -------------------------
        // 🔹 Constructors
        // -------------------------
        public ApplicationUser() : base()
        {
            Id = GenerateCustomUserId();
        }

        public ApplicationUser(string fullName, string? gender = null, string? address = null)
        {
            Id = GenerateCustomUserId();
            FullName = fullName;
            DateJoined = DateTime.UtcNow;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }
        

        // -------------------------
        // 🔹 Custom ID Generator
        // -------------------------
        private static string GenerateCustomUserId()
        {
            return $"USR-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";
        }
    }
}
