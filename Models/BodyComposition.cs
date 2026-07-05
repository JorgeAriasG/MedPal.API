using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class BodyComposition : IAuditableEntity, ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PatientDetails")]
        [Required]
        public int PatientDetailsId { get; set; }

        [Required]
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

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

        // InBody-specific fields (optional — not all clinics have InBody)
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
        public string? InBodyRawData { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public int? CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public int? LastModifiedByUserId { get; set; }

        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        public virtual PatientDetails PatientDetails { get; set; }
    }
}
