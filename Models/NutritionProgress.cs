using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class NutritionProgress : IAuditableEntity, ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PatientDetails")]
        [Required]
        public int PatientDetailsId { get; set; }

        [Required]
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        public decimal? Weight { get; set; }
        public decimal? BodyFatPercentage { get; set; }
        public decimal? MuscleMass { get; set; }
        public decimal? Waist { get; set; }
        public decimal? SkeletalMuscleMass { get; set; }
        public decimal? WaistCircumference { get; set; }

        public decimal? CaloriesConsumed { get; set; }
        public decimal? ProteinConsumed { get; set; }
        public decimal? CarbsConsumed { get; set; }
        public decimal? FatConsumed { get; set; }

        public int? WaterGlasses { get; set; }
        public int? ExerciseMinutes { get; set; }

        public int? Adherence { get; set; }

        [StringLength(2000)]
        public string? Observations { get; set; }

        public int? DietPlanId { get; set; }

        [StringLength(2000)]
        public string? Notes { get; set; }

        [StringLength(4000)]
        public string? Photos { get; set; }

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
