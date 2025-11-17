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
        public string Action { get; set; } = string.Empty;
        
        [Required]
        public string UserId { get; set; } = string.Empty;
        
        public string Module { get; set; } = string.Empty;  // Added default value
        
        public string Description { get; set; } = string.Empty;  // Added default value
        
        public string IpAddress { get; set; } = string.Empty;  // Added default value
        
        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }
        
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        
        // The module being changed, e.g. "Booking", "Service", "User"
        [Required]
        [MaxLength(100)]
        public string EntityType { get; set; } = string.Empty;
        
        // The record ID (Booking.Id, Service.Id, etc.)
        [Required]
        public string EntityId { get; set; } = string.Empty;
        
        // Message / details
        [Required]
        [MaxLength(2000)]
        public string Details { get; set; } = string.Empty;
    }
}