using System;

namespace MedPal.API.DTOs
{
    public class NutritionProgressReadDTO
    {
        public int Id { get; set; }
        public int PatientDetailsId { get; set; }
        public DateTime RecordedAt { get; set; }
        public decimal? Weight { get; set; }
        public decimal? BodyFatPercentage { get; set; }
        public decimal? MuscleMass { get; set; }
        public decimal? Waist { get; set; }
        public decimal? SkeletalMuscleMass { get; set; }
        public decimal? WaistCircumference { get; set; }
        public int? Adherence { get; set; }
        public string? Observations { get; set; }
        public int? DietPlanId { get; set; }
        public string? Photos { get; set; }
        public decimal? CaloriesConsumed { get; set; }
        public decimal? ProteinConsumed { get; set; }
        public decimal? CarbsConsumed { get; set; }
        public decimal? FatConsumed { get; set; }
        public int? WaterGlasses { get; set; }
        public int? ExerciseMinutes { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
