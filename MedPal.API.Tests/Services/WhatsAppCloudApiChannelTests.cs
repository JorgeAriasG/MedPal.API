using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace MedPal.API.Tests.Services
{
    public class WhatsAppCloudApiChannelTests
    {
        private readonly Mock<ILogger<WhatsAppCloudApiChannel>> _loggerMock;
        private readonly WhatsAppSettings _settings;

        public WhatsAppCloudApiChannelTests()
        {
            _loggerMock = new Mock<ILogger<WhatsAppCloudApiChannel>>();
            _settings = new WhatsAppSettings
            {
                Enabled = true,
                GraphUrl = "https://graph.facebook.com",
                ApiVersion = "v21.0",
                PhoneNumberId = "test-phone-id",
                AccessToken = "test-token",
                AppSecret = "test-secret",
                TemplateName = "appointment_reminder",
                RegistrationTemplateName = "patient_registration_link",
                TemplateLanguage = "es_MX",
                HttpTimeoutSeconds = 30
            };
        }

        private WhatsAppCloudApiChannel CreateChannel(MockHttpMessageHandler handler)
        {
            var httpClient = new HttpClient(handler);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient("WhatsApp")).Returns(httpClient);

            return new WhatsAppCloudApiChannel(
                httpClientFactoryMock.Object,
                Options.Create(_settings),
                _loggerMock.Object);
        }

        [Fact]
        public async Task SendAsync_WhenEnabled_ShouldSendTemplateMessage()
        {
            var responseJson = """
            {
                "messaging_product": "whatsapp",
                "contacts": [{"input": "+521234567890", "wa_id": "521234567890"}],
                "messages": [{"id": "wamid.test123"}]
            }
            """;

            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, responseJson);
            var httpClient = new HttpClient(handler);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient("WhatsApp")).Returns(httpClient);

            var channel = new WhatsAppCloudApiChannel(
                httpClientFactoryMock.Object,
                Options.Create(_settings),
                _loggerMock.Object);

            var message = new NotificationMessage
            {
                Recipient = "+521234567890",
                Type = NotificationType.WhatsApp,
                Body = "Juan | 20/01/2026 | 10:00 | Clínica Centro",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await channel.SendAsync(message);

            message.IsSent.Should().BeTrue();
            message.ProviderMessageId.Should().Be("wamid.test123");
            message.DeliveryStatus.Should().Be("sent");
            message.SentAt.Should().NotBeNull();
        }

        [Fact]
        public async Task SendAsync_WhenApiReturnsError_ShouldMarkFailed()
        {
            var errorJson = """
            {
                "error": {
                    "message": "Invalid phone number",
                    "type": "OAuthException",
                    "code": 100
                }
            }
            """;

            var handler = new MockHttpMessageHandler(HttpStatusCode.BadRequest, errorJson);
            var httpClient = new HttpClient(handler);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient("WhatsApp")).Returns(httpClient);

            var channel = new WhatsAppCloudApiChannel(
                httpClientFactoryMock.Object,
                Options.Create(_settings),
                _loggerMock.Object);

            var message = new NotificationMessage
            {
                Recipient = "+521234567890",
                Type = NotificationType.WhatsApp,
                Body = "Test",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await channel.SendAsync(message);

            message.IsSent.Should().BeFalse();
            message.DeliveryStatus.Should().Be("failed");
            message.ErrorDetail.Should().Contain("400");
        }

        [Fact]
        public async Task SendAsync_WhenDisabled_ShouldSkip()
        {
            _settings.Enabled = false;

            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, "{}");
            var httpClient = new HttpClient(handler);
            var httpClientFactoryMock = new Mock<IHttpClientFactory>();
            httpClientFactoryMock.Setup(f => f.CreateClient("WhatsApp")).Returns(httpClient);

            var channel = new WhatsAppCloudApiChannel(
                httpClientFactoryMock.Object,
                Options.Create(_settings),
                _loggerMock.Object);

            var message = new NotificationMessage
            {
                Recipient = "+521234567890",
                Type = NotificationType.WhatsApp,
                Body = "Test",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await channel.SendAsync(message);

            handler.RequestCount.Should().Be(0);
            message.IsSent.Should().BeFalse();
        }

        [Fact]
        public void CanHandle_WhatsAppType_ShouldReturnTrue()
        {
            var channel = new WhatsAppCloudApiChannel(
                Mock.Of<IHttpClientFactory>(),
                Options.Create(_settings),
                _loggerMock.Object);

            channel.CanHandle(NotificationType.WhatsApp).Should().BeTrue();
            channel.CanHandle(NotificationType.Email).Should().BeFalse();
        }

        [Fact]
        public void RegistrationTemplateName_ShouldDefaultToPatientRegistrationLink()
        {
            new WhatsAppSettings().RegistrationTemplateName.Should().Be("patient_registration_link");
        }

        [Fact]
        public async Task SendAsync_RegistrationTemplate_ShouldSendUrlButtonWithToken()
        {
            var responseJson = """
            {
                "messaging_product": "whatsapp",
                "contacts": [{"input": "+521234567890", "wa_id": "521234567890"}],
                "messages": [{"id": "wamid.reg123"}]
            }
            """;

            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, responseJson);
            var channel = CreateChannel(handler);

            var token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.payload.signature";
            var message = new NotificationMessage
            {
                Recipient = "+521234567890",
                Type = NotificationType.WhatsApp,
                Body = $"Juan | {token}",
                TemplateName = "patient_registration_link",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await channel.SendAsync(message);

            message.IsSent.Should().BeTrue();
            message.ProviderMessageId.Should().Be("wamid.reg123");
            handler.LastRequestBody.Should().NotBeNull();

            using var doc = JsonDocument.Parse(handler.LastRequestBody!);
            var root = doc.RootElement;
            root.GetProperty("template").GetProperty("name").GetString().Should().Be("patient_registration_link");

            var components = root.GetProperty("template").GetProperty("components");
            var bodyComponent = components.EnumerateArray()
                .First(c => c.GetProperty("type").GetString() == "body");
            var bodyParams = bodyComponent.GetProperty("parameters").EnumerateArray()
                .Select(p => p.GetProperty("text").GetString()).ToArray();
            bodyParams.Should().BeEquivalentTo(new[] { "Juan" });

            var buttonComponent = components.EnumerateArray()
                .First(c => c.GetProperty("type").GetString() == "button");
            buttonComponent.GetProperty("sub_type").GetString().Should().Be("url");
            buttonComponent.GetProperty("index").GetInt32().Should().Be(0);
            buttonComponent.GetProperty("parameters")[0].GetProperty("text").GetString().Should().Be(token);

            components.EnumerateArray().Should().NotContain(c =>
                c.GetProperty("type").GetString() == "button" &&
                c.GetProperty("sub_type").GetString() == "quick_reply");
        }

        [Fact]
        public async Task SendAsync_ReminderTemplate_ShouldKeepUrlButtonAtIndexTwo()
        {
            var responseJson = """
            {
                "messaging_product": "whatsapp",
                "contacts": [{"input": "+521234567890", "wa_id": "521234567890"}],
                "messages": [{"id": "wamid.rem123"}]
            }
            """;

            var handler = new MockHttpMessageHandler(HttpStatusCode.OK, responseJson);
            var channel = CreateChannel(handler);

            var message = new NotificationMessage
            {
                Recipient = "+521234567890",
                Type = NotificationType.WhatsApp,
                Body = "Juan | 20/01/2026 | 10:00 | Clínica Centro",
                AppointmentId = 123,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await channel.SendAsync(message);

            message.IsSent.Should().BeTrue();
            using var doc = JsonDocument.Parse(handler.LastRequestBody!);
            var root = doc.RootElement;
            root.GetProperty("template").GetProperty("name").GetString().Should().Be("appointment_reminder");

            var components = root.GetProperty("template").GetProperty("components");
            var bodyComponent = components.EnumerateArray()
                .First(c => c.GetProperty("type").GetString() == "body");
            bodyComponent.GetProperty("parameters").GetArrayLength().Should().Be(4);

            var buttons = components.EnumerateArray()
                .Where(c => c.GetProperty("type").GetString() == "button").ToArray();
            buttons.Should().HaveCount(3);
            buttons.Select(b => b.GetProperty("sub_type").GetString())
                .Should().ContainInOrder(new[] { "quick_reply", "quick_reply", "url" });
            buttons[2].GetProperty("index").GetInt32().Should().Be(2);
        }
    }

    internal class MockHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _responseBody;
        public int RequestCount { get; private set; }
        public string? LastRequestBody { get; private set; }

        public MockHttpMessageHandler(HttpStatusCode statusCode, string responseBody)
        {
            _statusCode = statusCode;
            _responseBody = responseBody;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            if (request.Content != null)
            {
                LastRequestBody = request.Content.ReadAsStringAsync(cancellationToken).GetAwaiter().GetResult();
            }
            return Task.FromResult(new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_responseBody)
            });
        }
    }
}
