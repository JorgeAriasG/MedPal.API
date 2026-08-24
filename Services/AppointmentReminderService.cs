using System;
using System.Linq;
using System.Threading;
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
    public interface IAppointmentReminderService
    {
        Task<int> SendRemindersAsync(CancellationToken ct = default);
        Task<bool> SendReminderForAppointmentAsync(int appointmentId, CancellationToken ct = default);
        Task<bool> SendConfirmationForAppointmentAsync(int appointmentId, CancellationToken ct = default);
    }

    public class AppointmentReminderService : IAppointmentReminderService
    {
        private readonly AppDbContext _context;
        private readonly INotificationChannel _channel;
        private readonly IOptions<WhatsAppSettings> _settings;
        private readonly ILogger<AppointmentReminderService> _logger;

        public AppointmentReminderService(
            AppDbContext context,
            INotificationChannel channel,
            IOptions<WhatsAppSettings> settings,
            ILogger<AppointmentReminderService> logger)
        {
            _context = context;
            _channel = channel;
            _settings = settings;
            _logger = logger;
        }

        public async Task<int> SendRemindersAsync(CancellationToken ct = default)
        {
            var windowHours = _settings.Value.ReminderWindowHoursAhead;
            var tomorrow = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(windowHours));

            var candidates = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.User)
                .Include(a => a.Clinic)
                .Where(a =>
                    a.Date == tomorrow &&
                    a.Status == AppointmentStatus.Scheduled &&
                    a.ReminderSentAt == null &&
                    a.Patient != null &&
                    a.Patient.IsWhatsAppConsented &&
                    !a.Patient.IsMarketingBlocked &&
                    a.Patient.Phone != null)
                .ToListAsync(ct);

            var sentCount = 0;

            foreach (var appointment in candidates)
            {
                try
                {
                    var notification = BuildReminderMessage(appointment);
                    await _channel.SendAsync(notification);

                    await _context.NotificationMessages.AddAsync(notification, ct);
                    appointment.ReminderSentAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync(ct);

                    sentCount++;
                    _logger.LogInformation(
                        "Reminder sent for Appointment {Id} to Patient {PatientId} via {Type}",
                        appointment.Id, appointment.PatientId, notification.Type);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send reminder for Appointment {Id} to Patient {PatientId}",
                        appointment.Id, appointment.PatientId);
                }
            }

            _logger.LogInformation("Reminder batch complete. Sent {Count}/{Total}", sentCount, candidates.Count);
            return sentCount;
        }

        public async Task<bool> SendReminderForAppointmentAsync(int appointmentId, CancellationToken ct = default)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.User)
                .Include(a => a.Clinic)
                .FirstOrDefaultAsync(a => a.Id == appointmentId, ct);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment {Id} not found", appointmentId);
                return false;
            }

            if (appointment.Patient == null || string.IsNullOrEmpty(appointment.Patient.Phone))
            {
                _logger.LogWarning("Appointment {Id} has no patient or phone", appointmentId);
                return false;
            }

            if (!appointment.Patient.IsWhatsAppConsented || appointment.Patient.IsMarketingBlocked)
            {
                _logger.LogWarning("Appointment {Id} patient has not consented or is marketing-blocked", appointmentId);
                return false;
            }

            if (appointment.ReminderSentAt != null)
            {
                _logger.LogInformation("Appointment {Id} already has reminder sent at {SentAt}", appointmentId, appointment.ReminderSentAt);
                return false;
            }

            var notification = BuildReminderMessage(appointment);
            await _channel.SendAsync(notification);

            await _context.NotificationMessages.AddAsync(notification, ct);
            appointment.ReminderSentAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Manual reminder sent for Appointment {Id} via {Type}", appointmentId, notification.Type);
            return true;
        }

        public async Task<bool> SendConfirmationForAppointmentAsync(int appointmentId, CancellationToken ct = default)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.User)
                .Include(a => a.Clinic)
                .FirstOrDefaultAsync(a => a.Id == appointmentId, ct);

            if (appointment == null)
            {
                _logger.LogWarning("Appointment {Id} not found for confirmation", appointmentId);
                return false;
            }

            if (appointment.Patient == null || string.IsNullOrEmpty(appointment.Patient.Phone))
            {
                _logger.LogWarning("Appointment {Id} has no patient or phone for confirmation", appointmentId);
                return false;
            }

            if (!appointment.Patient.IsWhatsAppConsented || appointment.Patient.IsMarketingBlocked)
            {
                _logger.LogWarning("Appointment {Id} patient has not consented for confirmation", appointmentId);
                return false;
            }

            var notification = BuildConfirmationMessage(appointment);
            await _channel.SendAsync(notification);

            await _context.NotificationMessages.AddAsync(notification, ct);
            await _context.SaveChangesAsync(ct);

            _logger.LogInformation("Confirmation sent for Appointment {Id} via {Type}", appointmentId, notification.Type);
            return true;
        }

        private NotificationMessage BuildReminderMessage(Appointment appointment)
        {
            var patientName = appointment.Patient?.Name ?? "Paciente";
            var date = appointment.Date.ToString("dd/MM/yyyy");
            var time = appointment.Time.ToString("HH:mm");
            var clinicName = appointment.Clinic?.Name ?? "la clínica";

            var body = $"{patientName} | {date} | {time} | {clinicName}";
            var subject = $"Recordatorio: Cita el {date} a las {time}";

            return new NotificationMessage
            {
                Recipient = PhoneNormalizer.Normalize(appointment.Patient?.Phone) ?? appointment.Patient?.Phone ?? string.Empty,
                Subject = subject,
                Type = NotificationType.WhatsApp,
                Body = body,
                TemplateName = _settings.Value.TemplateName,
                AppointmentId = appointment.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private NotificationMessage BuildConfirmationMessage(Appointment appointment)
        {
            var patientName = appointment.Patient?.Name ?? "Paciente";
            var date = appointment.Date.ToString("dd/MM/yyyy");
            var time = appointment.Time.ToString("HH:mm");
            var clinicName = appointment.Clinic?.Name ?? "la clínica";

            var body = $"{patientName} | {date} | {time} | {clinicName}";
            var subject = $"Confirmación: Cita el {date} a las {time}";

            return new NotificationMessage
            {
                Recipient = PhoneNormalizer.Normalize(appointment.Patient?.Phone) ?? appointment.Patient?.Phone ?? string.Empty,
                Subject = subject,
                Type = NotificationType.WhatsApp,
                Body = body,
                TemplateName = _settings.Value.ConfirmationTemplateName,
                AppointmentId = appointment.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
