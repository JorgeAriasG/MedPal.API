using System;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class PatientClinic : ISoftDelete, IAuditableEntity
    {
        public int PatientId { get; set; }
        public virtual Patient Patient { get; set; }
        public int ClinicId { get; set; }
        public virtual Clinic Clinic { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        // IAuditableEntity implementation
        public DateTime CreatedAt { get; set; }
        public int? CreatedByUserId { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public int? LastModifiedByUserId { get; set; }
    }
}
