using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FeesManagementSystem.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FeesManagementSystem.Background
{
    public class FeeNotificationJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FeeNotificationJob> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(2); // Check daily
        
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
                    await CheckOverdueFeesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred while checking overdue fees.");
                }

                _logger.LogInformation($"Fee Notification Job sleeping for {_checkInterval.TotalHours} hours.");
                await Task.Delay(_checkInterval, stoppingToken);
            }


        }

        private async Task CheckOverdueFeesAsync(CancellationToken stoppingToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var feeService = scope.ServiceProvider.GetRequiredService<IFeeService>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

                var overdueFees = await feeService.GetOverdueFeesAsync();
                
                if (overdueFees.Any())
                {
                    _logger.LogInformation($"Found {overdueFees.Count} overdue fees.");
                    foreach (var fee in overdueFees)
                    {
                        if (stoppingToken.IsCancellationRequested) break;

                        if (fee.Student != null)
                        {
                            var message = $"Dear {fee.Student.Name}, your fee of {fee.Amount:C} for {fee.FeeHead?.Name} was due on {fee.DueDate.ToShortDateString()}. Please pay immediately.";
                            
                            // Send SMS
                            if (!string.IsNullOrEmpty(fee.Student.PhoneNumber))
                            {
                                await notificationService.SendSmsAsync(fee.Student.PhoneNumber, message);
                            }

                            // Send Email
                            if (!string.IsNullOrEmpty(fee.Student.Email))
                            {
                                await notificationService.SendEmailAsync(fee.Student.Email, "Overdue Fee Alert", message);
                            }
                        }
                    }
                }
                else
                {
                    _logger.LogInformation("No overdue fees found.");
                }
            }
        }
    }
}
