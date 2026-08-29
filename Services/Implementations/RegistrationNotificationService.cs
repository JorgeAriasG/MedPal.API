using System;
using System.Threading.Tasks;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MedPal.API.Services.Implementations
{
    public class RegistrationNotificationService : IRegistrationNotificationService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly INotificationChannel _channel;
        private readonly IConfiguration _config;
        private readonly ILogger<RegistrationNotificationService> _logger;

        public RegistrationNotificationService(
            IPatientRepository patientRepository,
            INotificationChannel channel,
            IConfiguration config,
            ILogger<RegistrationNotificationService> logger)
        {
            _patientRepository = patientRepository;
            _channel = channel;
            _config = config;
            _logger = logger;
        }

        public async Task SendRegistrationLinkAsync(int patientId, string rawToken)
        {
            try
            {
                var patient = await _patientRepository.GetPatientByIdAsync(patientId);
                if (patient == null || string.IsNullOrWhiteSpace(patient.Phone))
                    return;

                var baseUrl = _config["Booking:CompleteRegistrationBaseUrl"] ?? "https://portal.clinicflow.com.mx/complete";
                var link = $"{baseUrl}?token={Uri.EscapeDataString(rawToken)}";

                var body = $"{patient.Name} | {link}";
                var templateName = _config["WhatsApp:RegistrationTemplateName"] ?? "appointment_created";

                var notification = new NotificationMessage
                {
                    Recipient = PhoneNormalizer.Normalize(patient.Phone) ?? patient.Phone,
                    Body = body,
                    Type = NotificationType.WhatsApp,
                    TemplateName = templateName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _channel.SendAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando link de completado de registro al paciente {PatientId}", patientId);
            }
        }
    }
}