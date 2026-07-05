using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class BodyCompositionWriteDTO
    {
        [Required(ErrorMessage = "El ID de PatientDetails es requerido")]
        public int PatientDetailsId { get; set; }

        [Required(ErrorMessage = "La fecha de medición es requerida")]
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [Range(0, 500)]
        public decimal? Weight { get; set; }

        [Range(0, 300)]
        public decimal? Height { get; set; }

        [Range(0, 100)]
        public decimal? Bmi { get; set; }

        [Range(0, 100)]
        public decimal? BodyFatPercentage { get; set; }

        [Range(0, 300)]
        public decimal? MuscleMass { get; set; }

        [Range(0, 50)]
        public decimal? BoneMass { get; set; }

        [Range(0, 100)]
        public decimal? BodyWaterPercentage { get; set; }

        [Range(1, 60)]
        public int? VisceralFat { get; set; }

        [Range(0, 5000)]
        public decimal? Bmr { get; set; }

        [Range(0, 120)]
        public int? MetabolicAge { get; set; }

        [Range(0, 100)]
        public decimal? ProteinMass { get; set; }

        [Range(0, 3)]
        public decimal? WaistHipRatio { get; set; }

        [Range(0, 300)]
        public decimal? BodyFatMass { get; set; }

        [Range(0, 100)]
        public decimal? TotalBodyWater { get; set; }

        [Range(0, 100)]
        public decimal? IntracellularWater { get; set; }

        [Range(0, 100)]
        public decimal? ExtracellularWater { get; set; }

        [Range(0, 3)]
        public decimal? EcwTbwRatio { get; set; }

        [Range(0, 50)]
        public decimal? Minerals { get; set; }

        [Range(0, 30)]
        public decimal? PhaseAngle { get; set; }

        [Range(0, 100)]
        public decimal? SegmentalLeanRightArm { get; set; }

        [Range(0, 100)]
        public decimal? SegmentalLeanLeftArm { get; set; }

        [Range(0, 200)]
        public decimal? SegmentalLeanTrunk { get; set; }

        [Range(0, 100)]
        public decimal? SegmentalLeanRightLeg { get; set; }

        [Range(0, 100)]
        public decimal? SegmentalLeanLeftLeg { get; set; }

        [StringLength(20)]
        public string? Source { get; set; }

        [StringLength(100)]
        public string? InbodyResultId { get; set; }

        public bool BwImported { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
