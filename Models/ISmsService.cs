namespace New_LeRayBookingSystem.Models// <-- Change this namespace
{
    public interface ISmsService
    {
        Task SendSmsAsync(string phoneNumber, string message);
    }
}