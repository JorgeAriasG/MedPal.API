using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Enums;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class Report : ISoftDelete, IAuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Patient")]
        [Required]
        public int PatientId { get; set; }

        [ForeignKey("MedicalHistory")]
        public int? MedicalHistoryId { get; set; }

        [ForeignKey("CreatedBy")]
        public int? CreatedByUserId { get; set; }

        [Required]
        public ReportType ReportType { get; set; }

        [Required]
        public string FileUrl { get; set; }

        public string Description { get; set; }

        public bool IsConfidential { get; set; } = true;

        public bool IsArcoReport { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        // IAuditableEntity implementation - Note: CreatedByUserId is already used as FK for CreatedBy navigation
        public int? UpdatedByUserId { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public int? LastModifiedByUserId { get; set; }

        public virtual Patient Patient { get; set; }
        public virtual MedicalHistory MedicalHistory { get; set; }
        public virtual User CreatedBy { get; set; }
    }
}