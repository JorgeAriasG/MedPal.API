using System;
using System.Threading.Tasks;
using MedPal.API.Data;
using MedPal.API.Enums;
using MedPal.API.Models;
using MedPal.API.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MedPal.API.Services
{
    public interface IWhatsAppInteractionHandler
    {
        Task HandleButtonResponseAsync(string fromPhone, string contactPhone, string wamid, string buttonId, string buttonText, string? repliedToWamid = null);
    }

    public class WhatsAppInteractionHandler : IWhatsAppInteractionHandler
    {
        private readonly AppDbContext _context;
        private readonly INotificationChannel _channel;
        private readonly IOptions<WhatsAppSettings> _settings;
        private readonly ILogger<WhatsAppInteractionHandler> _logger;

        public WhatsAppInteractionHandler(
            AppDbContext context,
            INotificationChannel channel,
            IOptions<WhatsAppSettings> settings,
            ILogger<WhatsAppInteractionHandler> logger)
        {
            _context = context;
            _channel = channel;
            _settings = settings;
            _logger = logger;
        }

        public async Task HandleButtonResponseAsync(
            string fromPhone, string contactPhone, string wamid, string buttonId, string buttonText, string? repliedToWamid = null)
        {
            var phone = PhoneNormalizer.ToE164(fromPhone);
            var phoneCandidates = new List<string>();
            if (!string.IsNullOrEmpty(phone))
            {
                phoneCandidates.Add(phone);
                phoneCandidates.Add(phone.TrimStart('+'));
                if (phone.Length >= 10)
                    phoneCandidates.Add(phone[^10..]);
            }
            if (!string.IsNullOrEmpty(fromPhone))
                phoneCandidates.Add(fromPhone);

            NotificationMessage? notification = null;

            if (!string.IsNullOrEmpty(repliedToWamid))
            {
                notification = await _context.NotificationMessages
                    .FirstOrDefaultAsync(n => n.ProviderMessageId == repliedToWamid && n.AppointmentId != null);

                if (notification == null)
                {
                    _logger.LogWarning("Button reply references unknown WAMID {Wamid}; falling back to phone match", repliedToWamid);
                }
            }

            if (notification == null)
            {
                if (phoneCandidates.Count == 0)
                {
                    _logger.LogWarning("Cannot normalize phone {Phone} for button response", fromPhone);
                    return;
                }

                notification = await _context.NotificationMessages
                    .Where(n => n.AppointmentId != null && n.IsSent == true)
                    .OrderByDescending(n => n.SentAt)
                    .FirstOrDefaultAsync(n => phoneCandidates.Contains(n.Recipient));
            }

            if (notification?.AppointmentId == null)
            {
                _logger.LogWarning("No appointment notification found for phone {Phone} (wamid={Wamid})", fromPhone, repliedToWamid);
                return;
            }

            var patient = await _context.Patients
                .FirstOrDefaultAsync(p => phoneCandidates.Contains(p.Phone));

            if (patient == null)
            {
                _logger.LogWarning("No patient found for phone {Phone} (wamid={Wamid})", fromPhone, repliedToWamid);
                return;
            }

            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Clinic)
                .FirstOrDefaultAsync(a => a.Id == notification.AppointmentId);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment {Id} not found for button response", notification.AppointmentId);
                return;
            }

            var existing = await _context.WhatsAppInteractions
                .FirstOrDefaultAsync(wi =>
                    wi.AppointmentId == appointment.Id &&
                    phoneCandidates.Contains(wi.PatientPhone) &&
                    wi.Wamid == wamid);

            if (existing != null)
            {
                _logger.LogDebug("Duplicate button response ignored for WAMID {Wamid}", wamid);
                return;
            }

            var interaction = new WhatsAppInteraction
            {
                AppointmentId = appointment.Id,
                PatientId = patient.Id,
                NotificationMessageId = notification.Id,
                ButtonId = buttonId,
                ButtonText = buttonText,
                PatientPhone = PhoneNormalizer.Normalize(fromPhone) ?? fromPhone,
                Wamid = wamid,
                ReceivedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            switch (buttonText.ToLower())
            {
                case "confirmar":
                    await HandleConfirmAsync(appointment, interaction);
                    break;
                case "cancelar":
                    await HandleCancelAsync(appointment, interaction);
                    break;
                default:
                    _logger.LogInformation("Unknown button text '{ButtonText}' for Appointment {Id}", buttonText, appointment.Id);
                    interaction.ActionTaken = "unknown";
                    break;
            }

            _context.WhatsAppInteractions.Add(interaction);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "WhatsApp interaction processed: Appointment={AppointmentId}, Button={ButtonId}, Action={Action}",
                appointment.Id, buttonId, interaction.ActionTaken);
        }

        private async Task HandleConfirmAsync(Appointment appointment, WhatsAppInteraction interaction)
        {
            if (appointment.Status == AppointmentStatus.Scheduled)
            {
                appointment.Status = AppointmentStatus.Confirmed;
                appointment.UpdatedAt = DateTime.UtcNow;
                interaction.ActionTaken = "Confirmed";

                var followUp = new NotificationMessage
                {
                    Recipient = interaction.PatientPhone,
                    Subject = "Cita confirmada",
                    Type = NotificationType.WhatsApp,
                    Body = BuildConfirmFollowUpBody(appointment),
                    TemplateName = _settings.Value.ConfirmationTemplateName,
                    AppointmentId = appointment.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _channel.SendAsync(followUp);
                await _context.NotificationMessages.AddAsync(followUp);

                interaction.FollowUpMessage = "Confirmation follow-up sent";
                _logger.LogInformation("Appointment {Id} confirmed by patient via WhatsApp", appointment.Id);
            }
            else
            {
                interaction.ActionTaken = "Confirm_Ignored";
                _logger.LogInformation("Appointment {Id} already in status {Status}, confirm ignored", appointment.Id, appointment.Status);
            }
        }

        private async Task HandleCancelAsync(Appointment appointment, WhatsAppInteraction interaction)
        {
            if (appointment.Status == AppointmentStatus.Scheduled ||
                appointment.Status == AppointmentStatus.Confirmed)
            {
                appointment.Status = AppointmentStatus.Cancelled;
                appointment.UpdatedAt = DateTime.UtcNow;
                interaction.ActionTaken = "Cancelled";

                var followUp = new NotificationMessage
                {
                    Recipient = interaction.PatientPhone,
                    Subject = "Cita cancelada",
                    Type = NotificationType.WhatsApp,
                    Body = BuildCancelFollowUpBody(appointment),
                    TemplateName = _settings.Value.CancelledTemplateName,
                    AppointmentId = appointment.Id,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _channel.SendAsync(followUp);
                await _context.NotificationMessages.AddAsync(followUp);

                interaction.FollowUpMessage = "Cancellation follow-up sent";
                _logger.LogInformation("Appointment {Id} cancelled by patient via WhatsApp", appointment.Id);
            }
            else
            {
                interaction.ActionTaken = "Cancel_Ignored";
                _logger.LogInformation("Appointment {Id} already in status {Status}, cancel ignored", appointment.Id, appointment.Status);
            }
        }

        private string BuildConfirmFollowUpBody(Appointment appointment)
        {
            var patientName = appointment.Patient?.Name ?? "Paciente";
            var date = appointment.Date.ToString("dd/MM/yyyy");
            var time = appointment.Time.ToString("HH:mm");
            var clinicName = appointment.Clinic?.Name ?? "la clínica";

            return $"{patientName} | {date} | {time} | {clinicName}";
        }

        private string BuildCancelFollowUpBody(Appointment appointment)
        {
            var patientName = appointment.Patient?.Name ?? "Paciente";
            var date = appointment.Date.ToString("dd/MM/yyyy");
            var time = appointment.Time.ToString("HH:mm");
            var clinicName = appointment.Clinic?.Name ?? "la clínica";

            return $"{patientName} | {date} | {time} | {clinicName}";
        }
    }
}
