using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class ClinicalAttachment : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("MedicalHistory")]
        [Required]
        public int MedicalHistoryId { get; set; }

        // Tipo de adjunto clínico: radio | photo | doc
        [StringLength(20)]
        public string Type { get; set; } = "doc";

        [Required]
        [StringLength(255)]
        public string FileName { get; set; }

        // Ruta relativa al Storage:AttachmentsPath
        [Required]
        [StringLength(1000)]
        public string StoragePath { get; set; }

        [StringLength(100)]
        public string MimeType { get; set; }

        public long Size { get; set; }

        public int? UploadedByUserId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Clínica propietaria (multi-tenancy)
        [ForeignKey("OwnerClinic")]
        public int? OwnerClinicId { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        // Navegaciones
        public virtual MedicalHistory MedicalHistory { get; set; }
        public virtual Clinic OwnerClinic { get; set; }
    }
}
