using ShoesStore.InterfaceRepositories;
using System.Net;
using System.Net.Mail;

namespace ShoesStore.Repositories
{
	public class EmailSender : IEmailSender
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<EmailSender> _logger;

		public EmailSender(IConfiguration configuration, ILogger<EmailSender> logger)
		{
			_configuration = configuration;
			_logger = logger;
		}

		public void SendEmail(string email, string subject, string message)
		{
			try
			{
				var fromEmail = _configuration["EmailSettings:Email"];
				var fromPassword = _configuration["EmailSettings:Password"];

				// Skip sending email if settings are not configured
				if (string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromPassword))
				{
					_logger.LogWarning("Email settings not configured. Skipping email send.");
					return;
				}

				var client = new SmtpClient("smtp.gmail.com", 587)
				{
					EnableSsl = true,
					UseDefaultCredentials = false,
					Credentials = new NetworkCredential(fromEmail, fromPassword)
				};

				var mailMessage = new MailMessage
				{
					From = new MailAddress(fromEmail),
					Subject = subject,
					Body = message,
					IsBodyHtml = true,
				};
				mailMessage.To.Add(email);

				client.Send(mailMessage);
			}
			catch (Exception ex)
			{
				// Log the error but don't throw it since email is not critical
				_logger.LogError($"Failed to send email: {ex.Message}");
			}
		}
	}
}
