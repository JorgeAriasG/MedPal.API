using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MedPal.API.Models;
using MedPal.API.Utils;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MedPal.API.Services
{
    public class WhatsAppCloudApiChannel : INotificationChannel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IOptions<WhatsAppSettings> _settings;
        private readonly ILogger<WhatsAppCloudApiChannel> _logger;

        public WhatsAppCloudApiChannel(
            IHttpClientFactory httpClientFactory,
            IOptions<WhatsAppSettings> settings,
            ILogger<WhatsAppCloudApiChannel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _settings = settings;
            _logger = logger;
        }

        public bool CanHandle(NotificationType type) =>
            type == NotificationType.WhatsApp;

        public async Task SendAsync(NotificationMessage message)
        {
            var settings = _settings.Value;

            if (!settings.Enabled)
            {
                _logger.LogWarning("WhatsApp channel is disabled. Skipping send to {Recipient}", message.Recipient);
                return;
            }

            var phone = PhoneNormalizer.ToE164(message.Recipient);
            if (string.IsNullOrEmpty(phone))
            {
                _logger.LogError("Cannot normalize phone number '{Recipient}' to E.164", message.Recipient);
                return;
            }

            var client = _httpClientFactory.CreateClient("WhatsApp");
            client.Timeout = TimeSpan.FromSeconds(settings.HttpTimeoutSeconds);

            var url = $"{settings.GraphUrl}/{settings.ApiVersion}/{settings.PhoneNumberId}/messages";

            var templateName = !string.IsNullOrEmpty(message.TemplateName)
                ? message.TemplateName
                : settings.TemplateName;

            var bodyParams = ExtractAllParameters(message.Body);
            var components = new List<object>
            {
                new
                {
                    type = "body",
                    parameters = bodyParams.Select(p => new { type = "text", text = p }).ToArray()
                }
            };

            var isReminderTemplate = templateName == settings.TemplateName;

            if (isReminderTemplate && message.AppointmentId.HasValue)
            {
                components.Add(new
                {
                    type = "button",
                    sub_type = "quick_reply",
                    index = 0,
                    parameters = Array.Empty<object>()
                });

                components.Add(new
                {
                    type = "button",
                    sub_type = "quick_reply",
                    index = 1,
                    parameters = Array.Empty<object>()
                });

                if (!string.IsNullOrEmpty(settings.RescheduleBaseUrl))
                {
                    components.Add(new
                    {
                        type = "button",
                        sub_type = "url",
                        index = 2,
                        parameters = new object[]
                        {
                            new { type = "text", text = message.AppointmentId.Value.ToString() }
                        }
                    });
                }
            }

            var body = new
            {
                messaging_product = "whatsapp",
                to = phone.TrimStart('+'),
                type = "template",
                template = new
                {
                    name = templateName,
                    language = new { code = settings.TemplateLanguage },
                    components = components.ToArray()
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = JsonContent.Create(body, options: new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                })
            };
            request.Headers.Add("Authorization", $"Bearer {settings.AccessToken}");

            try
            {
                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var doc = JsonDocument.Parse(responseBody);
                    if (doc.RootElement.TryGetProperty("messages", out var messages) &&
                        messages.GetArrayLength() > 0)
                    {
                        var wamid = messages[0].GetProperty("id").GetString();
                        message.ProviderMessageId = wamid;
                        message.IsSent = true;
                        message.SentAt = DateTime.UtcNow;
                        message.DeliveryStatus = "sent";

                        _logger.LogInformation("WhatsApp message sent to {Phone}. WAMID: {Wamid}", phone, wamid);
                    }
                }
                else
                {
                    message.IsSent = false;
                    message.DeliveryStatus = "failed";
                    message.ErrorDetail = $"HTTP {(int)response.StatusCode}: {responseBody}";

                    _logger.LogError("WhatsApp API error for {Phone}: {StatusCode} - {Body}",
                        phone, (int)response.StatusCode, responseBody);
                }
            }
            catch (TaskCanceledException)
            {
                message.IsSent = false;
                message.DeliveryStatus = "failed";
                message.ErrorDetail = "Request timeout";
                _logger.LogError("WhatsApp API timeout for {Phone}", phone);
            }
            catch (HttpRequestException ex)
            {
                message.IsSent = false;
                message.DeliveryStatus = "failed";
                message.ErrorDetail = ex.Message;
                _logger.LogError(ex, "WhatsApp API HTTP error for {Phone}", phone);
            }
        }

        private static string ExtractParameter(string body, int index)
        {
            if (string.IsNullOrEmpty(body))
                return string.Empty;

            var parts = body.Split(" | ", StringSplitOptions.RemoveEmptyEntries);
            return index <= parts.Length ? parts[index - 1].Trim() : string.Empty;
        }

        private static string[] ExtractAllParameters(string body)
        {
            if (string.IsNullOrEmpty(body))
                return Array.Empty<string>();

            return body.Split(" | ", StringSplitOptions.RemoveEmptyEntries)
                       .Select(p => p.Trim())
                       .ToArray();
        }
    }
}
