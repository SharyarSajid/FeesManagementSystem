
using FeesManagementSystem.Data;
using FeesManagementSystem.Services;
using Microsoft.EntityFrameworkCore;

namespace FeesManagementSystem.Background
{
    public class FeeNotificationJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FeeNotificationJob> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromDays(1);

        public FeeNotificationJob(IServiceProvider serviceProvider, ILogger<FeeNotificationJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Fee Notification Job started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CheckFeesAndNotifyAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking fees.");
                }

                _logger.LogInformation($"Job sleeping for {_checkInterval.TotalDays} day...");
                await Task.Delay(_checkInterval, stoppingToken);
            }
        }

        private async Task CheckFeesAndNotifyAsync(CancellationToken stoppingToken)
        {
            if (stoppingToken.IsCancellationRequested) return;

            using (var scope = _serviceProvider.CreateScope())
            {
                var _context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                DateTime today = DateTime.Today;

                var pendingFees = await _context.StudentFees
                    .Include(f => f.Student)
                    .Include(f => f.FeeHead)
                    .Where(f => !f.IsPaid)
                    .ToListAsync(stoppingToken);

                foreach (var fee in pendingFees)
                {
                    if (stoppingToken.IsCancellationRequested) return;
                    if (fee.Student == null || string.IsNullOrEmpty(fee.Student.Email)) continue;

                    string subject = "";
                    string message = "";
                    bool shouldSend = false;

                    // Scenario 1: 7 din pehle reminder
                    if (fee.DueDate.Date == today.AddDays(7).Date)
                    {
                        subject = "Upcoming Fee Reminder (1 Week Left)";
                        message = $"Dear {fee.Student.Name}, your fee of {fee.Amount} for {fee.FeeHead?.Name} is due on {fee.DueDate.ToShortDateString()}.";
                        shouldSend = true;
                    }
                    // Scenario 2: Overdue alert (Due date guzar gayi)
                    else if (fee.DueDate.Date < today.Date)
                    {
                        subject = "Overdue Fee Alert";
                        message = $"Dear {fee.Student.Name}, your fee of {fee.Amount} for {fee.FeeHead?.Name} was due on {fee.DueDate.ToShortDateString()}. Please pay immediately.";
                        shouldSend = true;
                    }

                    if (shouldSend)
                    {
                        try
                        {
                            await notificationService.SendEmailAsync(fee.Student.Email, subject, message);
                            if (!stoppingToken.IsCancellationRequested)
                            {
                                _logger.LogInformation($"Notification sent to {fee.Student.Email}");
                            }
                        }
                        catch (Exception ex)
                        {
                            if (!stoppingToken.IsCancellationRequested)
                            {
                                _logger.LogError($"Email error: {ex.Message}");
                            }
                        }
                    }
                }
            }
        }
    }

}
