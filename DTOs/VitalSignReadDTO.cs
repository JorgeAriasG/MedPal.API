using System;

namespace MedPal.API.DTOs
{
    public class VitalSignReadDTO
    {
        public int Id { get; set; }
        public int PatientDetailsId { get; set; }
        public int? SystolicBP { get; set; }
        public int? DiastolicBP { get; set; }
        public int? HeartRate { get; set; }
        public decimal? Temperature { get; set; }
        public int? RespiratoryRate { get; set; }
        public int? OxygenSaturation { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? Bmi { get; set; }
        public int? BloodGlucose { get; set; }
        public string? Notes { get; set; }
        public DateTime RecordedAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
