using FeesManagementSystem.Data;
using FeesManagementSystem.Models;

namespace FeesManagementSystem.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;
        private readonly IConfiguration _configuration;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger, IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            try 
            {
                var emailSettings = _configuration.GetSection("EmailSettings");
                var host = emailSettings["Host"];
                if (!string.IsNullOrEmpty(host) && host != "smtp.example.com")
                {
                    var port = int.Parse(emailSettings["Port"]);
                    var username = emailSettings["Username"];
                    var password = emailSettings["Password"];

                    using (var client = new System.Net.Mail.SmtpClient(host, port))
                    {
                        client.Credentials = new System.Net.NetworkCredential(username, password);
                        client.EnableSsl = true;

                        var mailMessage = new System.Net.Mail.MailMessage
                        {
                            From = new System.Net.Mail.MailAddress(username),
                            Subject = subject,
                            Body = message,
                            IsBodyHtml = true
                        };
                        mailMessage.To.Add(email);

                        await client.SendMailAsync(mailMessage);
                        _logger.LogInformation($"Email sent manually to {email}");
                    }
                }
                else
                {
                    _logger.LogWarning("Email settings not configured. Skipping real email send.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {email}");
               
            }

            // Still log to DB for record
            var log = new NotificationLog
            {
                Recipient = email,
                Type = "Email",
                Message = message,
                SentAt = DateTime.Now,
                Success = true 
            };
            
            _context.NotificationLogs.Add(log);
            await _context.SaveChangesAsync();
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            // Mock Implementation
            _logger.LogInformation($"Sending SMS to {phoneNumber}: {message}");

            var log = new NotificationLog
            {
                Recipient = phoneNumber,
                Type = "SMS",
                Message = message,
                SentAt = DateTime.Now,
                Success = true
            };

            _context.NotificationLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
