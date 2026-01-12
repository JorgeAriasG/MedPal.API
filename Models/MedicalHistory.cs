using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class MedicalHistory : IAuditableEntity, ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PatientDetails")]
        public int PatientDetailsId { get; set; }

        // Tipo de consulta/especialidad (Cardiología, Odontología, Nutrición, etc.)
        [Required]
        public string SpecialtyType { get; set; }

        // Datos JSON polimórficos para especialidades (Dental, Nutrición, etc.)
        public string SpecialtyData { get; set; }

        // Diagnóstico (reemplaza ConditionName)
        [Required]
        public string Diagnosis { get; set; }

        // Fecha de diagnóstico
        [Required]
        public DateTime DiagnosisDate { get; set; }

        // Notas clínicas (reemplaza DoctorNotes)
        public string ClinicalNotes { get; set; }

        // ID del profesional que realizó el diagnóstico
        [ForeignKey("User")]
        public int? HealthcareProfessionalId { get; set; }

        // Fecha de seguimiento recomendado
        public DateTime? FollowUpDate { get; set; }

        // Relación con Prescription (opcional - si se prescribió algo)
        [ForeignKey("Prescription")]
        public int? PrescriptionId { get; set; }

        // Datos de auditoría (cumplimiento LSSI-PC/NOM)
        [Required]
        public DateTime CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        // Usuario que modificó por última vez
        public int? LastModifiedByUserId { get; set; }

        // IAuditableEntity implementation
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? LastModifiedAt { get; set; }

        // Indicador de confidencialidad (para cumplir LSSI-PC)
        public bool IsConfidential { get; set; } = true;

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        // Navegaciones
        public virtual PatientDetails PatientDetails { get; set; }
        public virtual User HealthcareProfessional { get; set; }
        public virtual User LastModifiedByUser { get; set; }
        public virtual Prescription Prescription { get; set; }
    }
}