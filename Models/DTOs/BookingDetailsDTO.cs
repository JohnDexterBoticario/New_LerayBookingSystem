
namespace New_LeRayBookingSystem.Models.DTOs 
{
    public class BookingDetailsDto
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public ServiceDto? Service { get; set; }
    }
}