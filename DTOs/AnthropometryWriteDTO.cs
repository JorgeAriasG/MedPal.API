using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class AnthropometryWriteDTO
    {
        [Required(ErrorMessage = "El ID de PatientDetails es requerido")]
        public int PatientDetailsId { get; set; }

        [Required(ErrorMessage = "La fecha de medición es requerida")]
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [Range(0, 500)]
        public decimal? Weight { get; set; }

        [Range(0, 300)]
        public decimal? Height { get; set; }

        public decimal? Bmi { get; set; }

        [Range(0, 300)]
        public decimal? Waist { get; set; }

        [Range(0, 300)]
        public decimal? Hip { get; set; }

        public decimal? WaistHipRatio { get; set; }

        public decimal? WaistHeightRatio { get; set; }

        [Range(0, 100)]
        public decimal? MidArmCircumference { get; set; }

        [Range(0, 100)]
        public decimal? BodyFatPercentageEstimated { get; set; }

        [Range(0, 100)]
        public decimal? Neck { get; set; }

        [Range(0, 100)]
        public decimal? ShoulderBreadth { get; set; }

        [Range(0, 200)]
        public decimal? Chest { get; set; }

        [Range(0, 100)]
        public decimal? Arm { get; set; }

        [Range(0, 80)]
        public decimal? Forearm { get; set; }

        [Range(0, 30)]
        public decimal? Wrist { get; set; }

        [Range(0, 150)]
        public decimal? Thigh { get; set; }

        [Range(0, 80)]
        public decimal? Calf { get; set; }

        [Range(0, 100)]
        public decimal? TricepsSkinfold { get; set; }

        [Range(0, 100)]
        public decimal? BicepsSkinfold { get; set; }

        [Range(0, 100)]
        public decimal? SubscapularSkinfold { get; set; }

        [Range(0, 100)]
        public decimal? SuprailiacSkinfold { get; set; }

        [Range(0, 100)]
        public decimal? CalfSkinfold { get; set; }

        [Range(0, 100)]
        public decimal? ThighSkinfold { get; set; }

        [Range(0, 100)]
        public decimal? AbdominalSkinfold { get; set; }

        [Range(0, 100)]
        public decimal? PectoralSkinfold { get; set; }

        [Range(0, 100)]
        public decimal? AxillarySkinfold { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
