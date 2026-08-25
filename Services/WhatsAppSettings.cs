namespace MedPal.API.Services
{
    public class WhatsAppSettings
    {
        public bool Enabled { get; set; }
        public string GraphUrl { get; set; } = "https://graph.facebook.com";
        public string ApiVersion { get; set; } = "v21.0";
        public string PhoneNumberId { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string AppSecret { get; set; } = string.Empty;
        public string WebhookVerifyToken { get; set; } = string.Empty;
        public string TemplateName { get; set; } = "appointment_reminder";
        public string CreatedTemplateName { get; set; } = "appointment_created";
        public string ConfirmationTemplateName { get; set; } = "appointment_confirmation";
        public string CancelledTemplateName { get; set; } = "appointment_cancelled";
        public string TemplateLanguage { get; set; } = "es_MX";
        public string RescheduleBaseUrl { get; set; } = "https://portal.clinicflow.com.mx/reschedule";
        public int ReminderHour { get; set; } = 18;
        public int ReminderWindowHoursAhead { get; set; } = 24;
        public int CheckIntervalMinutes { get; set; } = 30;
        public int HttpTimeoutSeconds { get; set; } = 30;
    }
}
