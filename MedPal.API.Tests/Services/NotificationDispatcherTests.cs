using System.Threading.Tasks;
using FluentAssertions;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace MedPal.API.Tests.Services
{
    public class NotificationDispatcherTests
    {
        private readonly Mock<ILogger<NotificationDispatcher>> _loggerMock;

        public NotificationDispatcherTests()
        {
            _loggerMock = new Mock<ILogger<NotificationDispatcher>>();
        }

        [Fact]
        public async Task SendAsync_WhenWhatsAppType_ShouldRouteToWhatsAppChannel()
        {
            var whatsappChannel = new Mock<INotificationChannel>();
            whatsappChannel.Setup(c => c.CanHandle(NotificationType.WhatsApp)).Returns(true);

            var mockChannel = new Mock<INotificationChannel>();
            mockChannel.Setup(c => c.CanHandle(NotificationType.WhatsApp)).Returns(false);

            var dispatcher = new NotificationDispatcher(
                new[] { mockChannel.Object, whatsappChannel.Object },
                _loggerMock.Object);

            var message = new NotificationMessage
            {
                Type = NotificationType.WhatsApp,
                Recipient = "+521234567890",
                Body = "Test"
            };

            await dispatcher.SendAsync(message);

            whatsappChannel.Verify(c => c.SendAsync(message), Times.Once);
            mockChannel.Verify(c => c.SendAsync(It.IsAny<NotificationMessage>()), Times.Never);
        }

        [Fact]
        public async Task SendAsync_WhenEmailType_ShouldRouteToMockChannel()
        {
            var whatsappChannel = new Mock<INotificationChannel>();
            whatsappChannel.Setup(c => c.CanHandle(NotificationType.WhatsApp)).Returns(true);
            whatsappChannel.Setup(c => c.CanHandle(NotificationType.Email)).Returns(false);

            var mockChannel = new Mock<INotificationChannel>();
            mockChannel.Setup(c => c.CanHandle(NotificationType.Email)).Returns(true);

            var dispatcher = new NotificationDispatcher(
                new[] { whatsappChannel.Object, mockChannel.Object },
                _loggerMock.Object);

            var message = new NotificationMessage
            {
                Type = NotificationType.Email,
                Recipient = "test@test.com",
                Body = "Test"
            };

            await dispatcher.SendAsync(message);

            mockChannel.Verify(c => c.SendAsync(message), Times.Once);
            whatsappChannel.Verify(c => c.SendAsync(It.IsAny<NotificationMessage>()), Times.Never);
        }

        [Fact]
        public void CanHandle_AlwaysReturnsTrue()
        {
            var dispatcher = new NotificationDispatcher(
                new[] { Mock.Of<INotificationChannel>() },
                _loggerMock.Object);

            dispatcher.CanHandle(NotificationType.WhatsApp).Should().BeTrue();
            dispatcher.CanHandle(NotificationType.Email).Should().BeTrue();
        }
    }
}
