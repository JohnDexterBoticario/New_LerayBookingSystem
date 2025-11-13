namespace New_LeRayBookingSystem.Models
{
    public enum UserRole { Client, Admin, SuperAdmin }
    public enum AppointmentStatus { Pending, Confirmed, Cancelled, Completed, NoShow }
    public enum PaymentStatus { Pending, DownpaymentVerified, FullyPaid, Rejected }
    public enum NotificationType { SMS, Email, System }
}