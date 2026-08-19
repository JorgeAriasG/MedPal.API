using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/webhooks")]
    public class WhatsAppWebhookController : ControllerBase
    {
        private readonly WhatsAppSettings _settings;
        private readonly AppDbContext _context;
        private readonly ILogger<WhatsAppWebhookController> _logger;

        public WhatsAppWebhookController(
            IOptions<WhatsAppSettings> settings,
            AppDbContext context,
            ILogger<WhatsAppWebhookController> logger)
        {
            _settings = settings.Value;
            _context = context;
            _logger = logger;
        }

        [HttpGet("whatsapp")]
        public IActionResult Verify(
            [FromQuery] string? hub_mode,
            [FromQuery] string? hub_verify_token,
            [FromQuery] string? hub_challenge)
        {
            if (hub_mode == "subscribe" && hub_verify_token == _settings.WebhookVerifyToken)
            {
                _logger.LogInformation("WhatsApp webhook verified successfully");
                return Content(hub_challenge ?? string.Empty);
            }

            _logger.LogWarning("WhatsApp webhook verification failed: token mismatch");
            return Forbid();
        }

        [HttpPost("whatsapp")]
        public async Task<IActionResult> HandleWebhook()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            if (!VerifySignature(json))
            {
                _logger.LogWarning("WhatsApp webhook signature verification failed");
                return BadRequest(new { message = "Invalid signature" });
            }

            try
            {
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.TryGetProperty("entry", out var entries))
                {
                    foreach (var entry in entries.EnumerateArray())
                    {
                        if (entry.TryGetProperty("changes", out var changes))
                        {
                            foreach (var change in changes.EnumerateArray())
                            {
                                if (change.TryGetProperty("value", out var value) &&
                                    value.TryGetProperty("statuses", out var statuses))
                                {
                                    await ProcessStatusesAsync(statuses);
                                }
                            }
                        }
                    }
                }

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing WhatsApp webhook");
                return Ok();
            }
        }

        private async Task ProcessStatusesAsync(JsonElement statuses)
        {
            foreach (var status in statuses.EnumerateArray())
            {
                var wamid = status.TryGetProperty("id", out var idProp) ? idProp.GetString() : null;
                var statusValue = status.TryGetProperty("status", out var statusProp) ? statusProp.GetString() : null;
                var timestamp = status.TryGetProperty("timestamp", out var tsProp) ? tsProp.GetString() : null;

                if (string.IsNullOrEmpty(wamid) || string.IsNullOrEmpty(statusValue))
                    continue;

                var notification = await _context.NotificationMessages
                    .FirstOrDefaultAsync(n => n.ProviderMessageId == wamid);

                if (notification == null)
                {
                    _logger.LogDebug("No notification found for WAMID {Wamid}", wamid);
                    continue;
                }

                notification.DeliveryStatus = statusValue;
                notification.UpdatedAt = DateTime.UtcNow;

                if (statusValue == "failed" && status.TryGetProperty("errors", out var errors))
                {
                    notification.ErrorDetail = errors.ToString();
                    notification.IsSent = false;
                }

                _logger.LogInformation("WhatsApp status update for WAMID {Wamid}: {Status}", wamid, statusValue);
            }

            await _context.SaveChangesAsync();
        }

        private bool VerifySignature(string body)
        {
            if (string.IsNullOrEmpty(_settings.AppSecret))
                return true;

            var signatureHeader = Request.Headers["X-Hub-Signature-256"].FirstOrDefault();
            if (string.IsNullOrEmpty(signatureHeader))
                return false;

            var expectedSignature = signatureHeader.Replace("sha256=", string.Empty);

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.AppSecret));
            var hash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(body)));

            return string.Equals(expectedSignature, hash, StringComparison.OrdinalIgnoreCase);
        }
    }
}
