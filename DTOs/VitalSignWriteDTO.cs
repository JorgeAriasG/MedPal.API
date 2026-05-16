using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class VitalSignWriteDTO
    {
        [Required(ErrorMessage = "El ID de PatientDetails es requerido")]
        public int PatientDetailsId { get; set; }

        [Range(0, 300, ErrorMessage = "Presión sistólica debe estar entre 0 y 300 mmHg")]
        public int? SystolicBP { get; set; }

        [Range(0, 200, ErrorMessage = "Presión diastólica debe estar entre 0 y 200 mmHg")]
        public int? DiastolicBP { get; set; }

        [Range(0, 300, ErrorMessage = "Frecuencia cardíaca debe estar entre 0 y 300 lpm")]
        public int? HeartRate { get; set; }

        [Range(32, 45, ErrorMessage = "Temperatura debe estar entre 32 y 45 °C")]
        public decimal? Temperature { get; set; }

        [Range(0, 100, ErrorMessage = "Frecuencia respiratoria debe estar entre 0 y 100 rpm")]
        public int? RespiratoryRate { get; set; }

        [Range(0, 100, ErrorMessage = "Saturación de oxígeno debe estar entre 0 y 100%")]
        public int? OxygenSaturation { get; set; }

        [Range(0, 500, ErrorMessage = "Peso debe estar entre 0 y 500 kg")]
        public decimal? Weight { get; set; }

        [Range(0, 300, ErrorMessage = "Altura debe estar entre 0 y 300 cm")]
        public decimal? Height { get; set; }

        public decimal? Bmi { get; set; }

        [Range(0, 1000, ErrorMessage = "Glucosa debe estar entre 0 y 1000 mg/dL")]
        public int? BloodGlucose { get; set; }

        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string? Notes { get; set; }

        [Required(ErrorMessage = "La fecha de registro es requerida")]
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    }
}
