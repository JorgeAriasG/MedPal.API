using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class NutritionProgressWriteDTO
    {
        [Required(ErrorMessage = "El ID de PatientDetails es requerido")]
        public int PatientDetailsId { get; set; }

        [Required(ErrorMessage = "La fecha de registro es requerida")]
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [Range(0, 500)]
        public decimal? Weight { get; set; }

        [Range(0, 100)]
        public decimal? BodyFatPercentage { get; set; }

        [Range(0, 300)]
        public decimal? MuscleMass { get; set; }

        [Range(0, 300)]
        public decimal? Waist { get; set; }

        [Range(0, 300)]
        public decimal? SkeletalMuscleMass { get; set; }

        [Range(0, 300)]
        public decimal? WaistCircumference { get; set; }

        [Range(0, 100)]
        public int? Adherence { get; set; }

        [StringLength(2000)]
        public string? Observations { get; set; }

        public int? DietPlanId { get; set; }

        [StringLength(4000)]
        public string? Photos { get; set; }

        [Range(0, 10000)]
        public decimal? CaloriesConsumed { get; set; }

        [Range(0, 1000)]
        public decimal? ProteinConsumed { get; set; }

        [Range(0, 1000)]
        public decimal? CarbsConsumed { get; set; }

        [Range(0, 1000)]
        public decimal? FatConsumed { get; set; }

        [Range(0, 50)]
        public int? WaterGlasses { get; set; }

        [Range(0, 600)]
        public int? ExerciseMinutes { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }
    }
}
