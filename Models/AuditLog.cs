using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace New_LeRayBookingSystem.Models
{
    public class AuditLog
    {
        [Key]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required]
        [MaxLength(100)]
        public string? Action { get; set; }  // e.g., "Created", "Updated", "Deleted"

        [Required]
        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(100)]
        public string? EntityType { get; set; }  // e.g., "Service", "Booking"

        [Required]
        public string? EntityId { get; set; }

        [Required]
        [MaxLength(2000)]
        public string? Details { get; set; }  // Optional details about the action
    }
}
