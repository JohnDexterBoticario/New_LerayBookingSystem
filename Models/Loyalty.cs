using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace New_LeRayBookingSystem.Models
{
    public class Loyalty
    {
        [Key]
        public int Id { get; set; }

        public int Points { get; set; } = 0;

        // Foreign key to ApplicationUser
        public string? UserId { get; set; } // <-- FIX (removed 'required')

        [ForeignKey(nameof(UserId))]
        public ApplicationUser? User { get; set; } // <-- FIX (removed 'required')
    }
}