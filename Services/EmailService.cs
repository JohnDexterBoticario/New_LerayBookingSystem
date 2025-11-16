using New_LeRayBookingSystem.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace New_LeRayBookingSystem.Services
{
    public class EmailService : IEmailService, IEmailSender
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string messageHtml)
        {
            if (string.IsNullOrWhiteSpace(toEmail))
                throw new ArgumentNullException(nameof(toEmail), "Recipient email cannot be null or empty.");

            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = messageHtml,
                TextBody = StripHtmlTags(messageHtml)
            };

            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();

            // Automatically select secure socket options
            SecureSocketOptions secureSocket;
            if (_emailSettings.SmtpPort == 587)
            {
                secureSocket = SecureSocketOptions.StartTls; // Gmail and most servers on 587
            }
            else if (_emailSettings.SmtpPort == 465)
            {
                secureSocket = SecureSocketOptions.SslOnConnect; // Gmail SSL port
            }
            else
            {
                // Use UseSSL property for custom ports
                secureSocket = _emailSettings.UseSSL ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTls;
            }

            await smtp.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.SmtpPort, secureSocket);
            await smtp.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        private static string StripHtmlTags(string html)
        {
            if (string.IsNullOrWhiteSpace(html)) return string.Empty;
            return System.Text.RegularExpressions.Regex.Replace(html, "<[^>]*>", string.Empty);
        }
    }
}
