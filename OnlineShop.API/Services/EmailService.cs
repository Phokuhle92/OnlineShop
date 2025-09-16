using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace OnlineShop.API.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            if (string.IsNullOrEmpty(toEmail))
                throw new ArgumentException("Recipient email cannot be null or empty.", nameof(toEmail));

            var emailSettings = _config.GetSection("EmailSettings");
            var senderEmail = emailSettings["SenderEmail"];
            var senderPassword = emailSettings["SenderPassword"];
            var smtpHost = emailSettings["SmtpHost"];
            var smtpPortString = emailSettings["SmtpPort"];

            if (string.IsNullOrEmpty(senderEmail) ||
                string.IsNullOrEmpty(senderPassword) ||
                string.IsNullOrEmpty(smtpHost) ||
                string.IsNullOrEmpty(smtpPortString) ||
                !int.TryParse(smtpPortString, out int smtpPort))
            {
                throw new InvalidOperationException("SMTP configuration is missing or invalid.");
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Shop Name", senderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            message.Body = new TextPart("plain") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, MailKit.Security.SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(senderEmail, senderPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

    }
}
