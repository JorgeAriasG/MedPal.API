namespace MedPal.API.DTOs
{
    public class AppointmentWriteDTO
    {
        public int? PatientId { get; set; }
        public int? UserId { get; set; }
        public int? ClinicId { get; set; }
        public string Status { get; set; }
        public string Notes { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly Time { get; set; }
        /// <summary>Duración en minutos. Debe estar entre 15 y 120.</summary>
        public int DurationMinutes { get; set; } = 30;
    }
}