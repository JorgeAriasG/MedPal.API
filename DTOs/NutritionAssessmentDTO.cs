using System;

namespace MedPal.API.DTOs
{
    public class NutritionAssessmentDTO
    {
        public int PatientDetailsId { get; set; }
        public decimal? Bmi { get; set; }
        public string? BmiClassification { get; set; }
        public decimal? BodyFatPercentage { get; set; }
        public string? BodyFatClassification { get; set; }
        public decimal? Bmr { get; set; }
        public int? MetabolicAge { get; set; }
        public decimal? WaistHipRatio { get; set; }
        public string? WhrClassification { get; set; }
        public decimal? EstimatedDailyCalories { get; set; }
        public decimal? RecommendedProteinG { get; set; }
        public decimal? RecommendedCarbsG { get; set; }
        public decimal? RecommendedFatG { get; set; }
        public string? AssessmentDate { get; set; }
    }
}
