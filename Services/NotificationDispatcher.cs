using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedPal.API.Models;
using Microsoft.Extensions.Logging;

namespace MedPal.API.Services
{
    public class NotificationDispatcher : INotificationChannel
    {
        private readonly IEnumerable<INotificationChannel> _channels;
        private readonly ILogger<NotificationDispatcher> _logger;

        public NotificationDispatcher(
            IEnumerable<INotificationChannel> channels,
            ILogger<NotificationDispatcher> logger)
        {
            _channels = channels;
            _logger = logger;
        }

        public bool CanHandle(NotificationType type) => true;

        public async Task SendAsync(NotificationMessage message)
        {
            var channel = _channels.FirstOrDefault(c => c.CanHandle(message.Type));

            if (channel == null)
            {
                _logger.LogWarning("No notification channel found for type {Type}. Falling back to first available.", message.Type);
                channel = _channels.FirstOrDefault();
            }

            if (channel == null)
            {
                _logger.LogError("No notification channels registered. Cannot send notification.");
                return;
            }

            _logger.LogInformation("Dispatching {Type} notification to {Channel} for {Recipient}",
                message.Type, channel.GetType().Name, message.Recipient);

            await channel.SendAsync(message);
        }
    }
}
