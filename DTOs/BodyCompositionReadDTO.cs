using System;

namespace MedPal.API.DTOs
{
    public class BodyCompositionReadDTO
    {
        public int Id { get; set; }
        public int PatientDetailsId { get; set; }
        public DateTime RecordedAt { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? Bmi { get; set; }
        public decimal? BodyFatPercentage { get; set; }
        public decimal? MuscleMass { get; set; }
        public decimal? BoneMass { get; set; }
        public decimal? BodyWaterPercentage { get; set; }
        public int? VisceralFat { get; set; }
        public decimal? Bmr { get; set; }
        public int? MetabolicAge { get; set; }
        public decimal? ProteinMass { get; set; }
        public decimal? WaistHipRatio { get; set; }
        public decimal? BodyFatMass { get; set; }
        public decimal? TotalBodyWater { get; set; }
        public decimal? IntracellularWater { get; set; }
        public decimal? ExtracellularWater { get; set; }
        public decimal? EcwTbwRatio { get; set; }
        public decimal? Minerals { get; set; }
        public decimal? PhaseAngle { get; set; }
        public decimal? SegmentalLeanRightArm { get; set; }
        public decimal? SegmentalLeanLeftArm { get; set; }
        public decimal? SegmentalLeanTrunk { get; set; }
        public decimal? SegmentalLeanRightLeg { get; set; }
        public decimal? SegmentalLeanLeftLeg { get; set; }
        public string? Source { get; set; }
        public string? InbodyResultId { get; set; }
        public bool BwImported { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
