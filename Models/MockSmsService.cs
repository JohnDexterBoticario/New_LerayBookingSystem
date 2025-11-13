using System.Diagnostics;

namespace New_LeRayBookingSystem.Models
{
    public class MockSmsService : ISmsService
    {
        public Task SendSmsAsync(string phoneNumber, string message)
        {
            // MOCK IMPLEMENTATION: Log to console instead of sending SMS
            Debug.WriteLine("--- MOCK SMS SERVICE ---");
            Debug.WriteLine($"To: {phoneNumber}");
            Debug.WriteLine($"Message: {message}");
            Debug.WriteLine("--------------------------");
            return Task.CompletedTask;
        }
    }
}