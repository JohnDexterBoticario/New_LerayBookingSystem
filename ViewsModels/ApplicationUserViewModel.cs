using System.ComponentModel.DataAnnotations;

namespace New_LeRayBookingSystem.ViewModels
{
    public class ApplicationUserViewModel
    {
        [Required]
        public required string Id { get; set; }

        [Required]
        [Display(Name = "Full Name")]
        public required string FullName { get; set; }

        [Display(Name = "Date Joined")]
        public DateTime DateJoined { get; set; } = DateTime.Now;
    }
}
