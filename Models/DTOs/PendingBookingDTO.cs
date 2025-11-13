namespace New_LeRayBookingSystem.Models.DTOs 
{
    // This DTO is returned after the initial booking creation
    public class PendingBookingDto
    {
        public int BookingId { get; set; }
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal DownPaymentAmount { get; set; }
        public string PaymentInstructions { get; set; } = string.Empty;
    }
}