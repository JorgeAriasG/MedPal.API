using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MedPal.API.Services
{
    public class AppointmentReminderJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppointmentReminderJob> _logger;
        private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(30);

        public AppointmentReminderJob(IServiceProvider serviceProvider, ILogger<AppointmentReminderJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Appointment Reminder Service is starting.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _logger.LogInformation("Checking for upcoming appointment reminders...");
                    using var scope = _serviceProvider.CreateScope();
                    var service = scope.ServiceProvider.GetRequiredService<IAppointmentReminderService>();
                    var sent = await service.SendRemindersAsync(stoppingToken);
                    _logger.LogInformation("Reminder batch: {Sent} messages sent", sent);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing appointment reminders.");
                }

                await Task.Delay(_checkInterval, stoppingToken);
            }
        }
    }
}
