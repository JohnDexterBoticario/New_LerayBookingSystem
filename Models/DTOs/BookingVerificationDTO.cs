
namespace New_LeRayBookingSystem.Models.DTOs
{
    public class BookingVerificationDto
    {
        public int BookingId { get; set; }
        public DateTime StartTime { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal DownPaymentAmount { get; set; }
        public string PaymentReceiptUrl { get; set; } = string.Empty;
    }
}