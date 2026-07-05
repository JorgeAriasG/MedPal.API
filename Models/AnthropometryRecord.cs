using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class AnthropometryRecord : IAuditableEntity, ISoftDelete
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
        public decimal? Waist { get; set; }
        public decimal? Hip { get; set; }
        public decimal? WaistHipRatio { get; set; }
        public decimal? WaistHeightRatio { get; set; }
        public decimal? MidArmCircumference { get; set; }
        public decimal? BodyFatPercentageEstimated { get; set; }
        public decimal? Neck { get; set; }
        public decimal? ShoulderBreadth { get; set; }
        public decimal? Chest { get; set; }
        public decimal? Arm { get; set; }
        public decimal? Forearm { get; set; }
        public decimal? Wrist { get; set; }
        public decimal? Thigh { get; set; }
        public decimal? Calf { get; set; }

        public decimal? TricepsSkinfold { get; set; }
        public decimal? BicepsSkinfold { get; set; }
        public decimal? SubscapularSkinfold { get; set; }
        public decimal? SuprailiacSkinfold { get; set; }
        public decimal? CalfSkinfold { get; set; }
        public decimal? ThighSkinfold { get; set; }
        public decimal? AbdominalSkinfold { get; set; }
        public decimal? PectoralSkinfold { get; set; }
        public decimal? AxillarySkinfold { get; set; }

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
