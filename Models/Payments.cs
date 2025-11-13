using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace New_LeRayBookingSystem.Models
{
    public class Payments
    {
        [Key]
        public int Id { get; set; } // Primary Key

        [Required]
        public string? UserId { get; set; }  // Nullable for EF compatibility

        [ForeignKey("UserId")]
        public ApplicationUser? User { get; set; }  // Nullable navigation property

        [Required]
        [MaxLength(200)]
        public string? Description { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Amount { get; set; }

        public DateTime PaymentDate { get; set; } = DateTime.Now;

        [Required]
        [MaxLength(50)]
        public string? PaymentMethod { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Pending";

        [Required]
        [MaxLength(255)]
        public string? ReceiptPath { get; set; }
    }
}
