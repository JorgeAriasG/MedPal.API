using System;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// Bloqueo de horario. Date se serializa como "yyyy-MM-dd" y StartTime/EndTime como "HH:mm"
    /// por los converters globales de System.Text.Json (ver Program.cs), igual que AppointmentReadDTO.
    /// </summary>
    public class AvailabilityBlockReadDTO
    {
        public int Id { get; set; }
        public int ClinicId { get; set; }
        public int? DoctorId { get; set; }
        public string? DoctorName { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Reason { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}