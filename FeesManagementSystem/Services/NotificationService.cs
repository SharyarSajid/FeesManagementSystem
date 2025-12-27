using System;
using System.Threading.Tasks;
using FeesManagementSystem.Data;
using FeesManagementSystem.Models;
using Microsoft.Extensions.Logging;

namespace FeesManagementSystem.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ApplicationDbContext context, ILogger<NotificationService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            // Mock Implementation
            _logger.LogInformation($"Sending Email to {email}: {subject}");
            
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
