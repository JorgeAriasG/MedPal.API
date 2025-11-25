using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedPal.API.Models
{
    public class MedicalHistory
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PatientDetails")]
        public int PatientDetailsId { get; set; }

        // Tipo de consulta/especialidad (Cardiología, Odontología, Nutrición, etc.)
        [Required]
        public string SpecialtyType { get; set; }

        // Diagnóstico (reemplaza ConditionName)
        [Required]
        public string Diagnosis { get; set; }

        // Fecha de diagnóstico
        [Required]
        public DateTime DiagnosisDate { get; set; }

        // Plan de tratamiento
        [Required]
        public string TreatmentPlan { get; set; }

        // Medicamentos prescritos
        public string PrescribedMedications { get; set; }

        // Notas clínicas (reemplaza DoctorNotes)
        public string ClinicalNotes { get; set; }

        // ID del profesional que realizó el diagnóstico
        [ForeignKey("User")]
        public int? HealthcareProfessionalId { get; set; }

        // Estado del tratamiento (Activo, Completado, Suspendido, etc.)
        public string TreatmentStatus { get; set; }

        // Fecha de seguimiento recomendado
        public DateTime? FollowUpDate { get; set; }

        // Datos de auditoría (cumplimiento LSSI-PC/NOM)
        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        // Usuario que modificó por última vez
        public int? LastModifiedByUserId { get; set; }

        // Indicador de confidencialidad (para cumplir LSSI-PC)
        public bool IsConfidential { get; set; } = true;

        // Navegaciones
        public virtual PatientDetails PatientDetails { get; set; }
        public virtual User HealthcareProfessional { get; set; }
        public virtual User LastModifiedByUser { get; set; }
    }
}