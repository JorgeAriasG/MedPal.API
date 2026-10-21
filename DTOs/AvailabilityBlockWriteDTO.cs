using System;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// Creación de un bloqueo de horario. Date ("yyyy-MM-dd") y StartTime/EndTime ("HH:mm")
    /// se deserializan mediante los converters globales de System.Text.Json.
    /// </summary>
    public class AvailabilityBlockWriteDTO
    {
        public int ClinicId { get; set; }

        /// <summary>Null = bloqueo de toda la clínica.</summary>
        public int? DoctorId { get; set; }

        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string? Reason { get; set; }
    }
}