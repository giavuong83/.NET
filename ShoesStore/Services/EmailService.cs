using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace ShoesStore.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _fromEmailAddress;
        private readonly string _fromEmailPassword;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _smtpServer = "smtp.gmail.com";
            _smtpPort = 587;
            _fromEmailAddress = configuration["EmailSettings:Email"];
            _fromEmailPassword = configuration["EmailSettings:Password"];
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                _logger.LogInformation($"Attempting to send email to {to}");
                
                var message = new MailMessage
                {
                    From = new MailAddress(_fromEmailAddress),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true,
                };

                message.To.Add(new MailAddress(to));

                using (var client = new SmtpClient(_smtpServer, _smtpPort))
                {
                    client.EnableSsl = true;
                    client.UseDefaultCredentials = false;
                    client.DeliveryMethod = SmtpDeliveryMethod.Network;
                    client.Credentials = new NetworkCredential(_fromEmailAddress, _fromEmailPassword);

                    _logger.LogInformation("Sending email...");
                    await client.SendMailAsync(message);
                    _logger.LogInformation("Email sent successfully");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to send email: {ex.Message}");
                _logger.LogError($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }
    }
} 