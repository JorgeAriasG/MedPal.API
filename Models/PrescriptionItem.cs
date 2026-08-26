using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class PrescriptionItem : ISoftDelete, IAuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Prescription")]
        public int PrescriptionId { get; set; }

        [Required]
        public string MedicationName { get; set; }

        [Required]
        public string Dosage { get; set; }

        [Required]
        public string Frequency { get; set; }

        [Required]
        public string Duration { get; set; }

        public string Instructions { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime? UpdatedAt { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        // IAuditableEntity implementation
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public int? LastModifiedByUserId { get; set; }

        // Navigation
        public virtual Prescription Prescription { get; set; }
    }
}
