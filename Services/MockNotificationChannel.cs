using System;
using System.Threading.Tasks;
using MedPal.API.Models;
using Microsoft.Extensions.Logging;

namespace MedPal.API.Services
{
    public class MockNotificationChannel : INotificationChannel
    {
        private readonly ILogger<MockNotificationChannel> _logger;

        public MockNotificationChannel(ILogger<MockNotificationChannel> logger)
        {
            _logger = logger;
        }

        public bool CanHandle(NotificationType type)
        {
            return true; // Mock handles everything
        }

        public Task SendAsync(NotificationMessage message)
        {
            _logger.LogInformation("================================================");
            _logger.LogInformation($"[MOCK NOTIFICATION] To: {message.Recipient}");
            _logger.LogInformation($"Type: {message.Type} | Subject: {message.Subject}");
            _logger.LogInformation($"Body: {message.Body}");
            _logger.LogInformation("================================================");

            return Task.CompletedTask;
        }
    }
}
