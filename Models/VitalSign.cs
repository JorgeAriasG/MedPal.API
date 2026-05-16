using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class VitalSign : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PatientDetails")]
        [Required]
        public int PatientDetailsId { get; set; }

        public int? SystolicBP { get; set; }
        public int? DiastolicBP { get; set; }
        public int? HeartRate { get; set; }
        public decimal? Temperature { get; set; }
        public int? RespiratoryRate { get; set; }
        public int? OxygenSaturation { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Height { get; set; }
        public decimal? Bmi { get; set; }
        public int? BloodGlucose { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }

        [Required]
        public DateTime RecordedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        public virtual PatientDetails PatientDetails { get; set; }
    }
}
