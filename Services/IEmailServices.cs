namespace New_LeRayBookingSystem.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string messageHtml);
        

    }
}
