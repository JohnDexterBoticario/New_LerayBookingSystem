using New_LeRayBookingSystem.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace New_LeRayBookingSystem.Services
{
    // ✅ Implements both your custom IEmailService and Identity’s IEmailSender
    public class EmailService : IEmailService, IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        // ✅ This method supports both your system and Identity pages
        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentNullException(nameof(toEmail), "Recipient email cannot be null or empty.");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = htmlMessage,
                TextBody = StripHtmlTags(htmlMessage)
            };

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            var secureSocket = _emailSettings.UseSSL
                ? SecureSocketOptions.SslOnConnect // Port 465
                : SecureSocketOptions.StartTls;    // Port 587

            await smtp.ConnectAsync(
                _emailSettings.SmtpServer,
                _emailSettings.SmtpPort,
                secureSocket
            );

            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        // Optional helper for fallback plain text body
        private static string StripHtmlTags(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", string.Empty);
        }
    }
}
