using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace New_LeRayBookingSystem.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Column(TypeName = "decimal(10, 2)")]
        public decimal Price { get; set; }

        public TimeSpan Duration { get; set; }

        [Required]
        public string CreatedBy { get; set; } = string.Empty;

        [ForeignKey("CreatedBy")]
        public ApplicationUser? CreatedByUser { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        public DateTime LastUpdatedAt { get; set; } = DateTime.Now;

        [Required]
        public string UpdatedBy { get; set; } = string.Empty;

        [Required]
        public string LastUpdatedBy { get; set; } = string.Empty;

        [ForeignKey("UpdatedBy")]
        public ApplicationUser? UpdatedByUser { get; set; }

        [ForeignKey("LastUpdatedBy")]
        public ApplicationUser? LastUpdatedByUser { get; set; }

        [Required]
        public string PublicServiceCode { get; set; } = string.Empty;

        [Required]
        public string ServiceName { get; set; } = string.Empty;

        [Required]
        public string ImagePath { get; set; } = string.Empty;

        [Required]
        public string Category { get; set; } = string.Empty;    

        public bool IsActive { get; set; } = true;
    }
}
