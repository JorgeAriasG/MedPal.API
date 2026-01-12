using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public enum PrescriptionStatus
    {
        Active,
        Dispensed,
        Cancelled,
        Expired
    }

    public class Prescription : ISoftDelete, IAuditableEntity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UniqueCode { get; set; } = Guid.NewGuid();

        [ForeignKey("Doctor")]
        public int DoctorId { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [Required]
        public string Diagnosis { get; set; }

        public string Notes { get; set; }

        [Required]
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime ExpiresAt { get; set; }

        [Required]
        public PrescriptionStatus Status { get; set; } = PrescriptionStatus.Active;

        // Relación con MedicalHistory (opcional - puede no estar vinculado a una consulta)
        [ForeignKey("MedicalHistory")]
        public int? MedicalHistoryId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        // IAuditableEntity implementation
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public int? LastModifiedByUserId { get; set; }

        // Navigations
        public virtual User Doctor { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
        public virtual MedicalHistory MedicalHistory { get; set; }
    }
}
