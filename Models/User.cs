using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; } // "Doctor", "Nutriologist", "Dentist", "Patient", "Admin"

        // Profesión/Especialidad (si es healthcare professional)
        public string Specialty { get; set; } // "Cardiología", "Odontología", "Nutrición", etc.

        // Número de cédula profesional (para validación en México)
        public string ProfessionalLicenseNumber { get; set; }

        // Estado activo/inactivo (cumplimiento LSSI-PC)
        public bool IsActive { get; set; } = true;

        // Soft delete flag
        public bool IsDeleted { get; set; } = false;

        // Fecha de último acceso (auditoría)
        public DateTime? LastAccessAt { get; set; }

        // Indica si el usuario aceptó términos de privacidad
        public bool HasAcceptedPrivacyTerms { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        // Usuario que desactivó la cuenta (si aplica)
        public int? DeactivatedByUserId { get; set; }

        // Fecha de borrado lógico
        public DateTime? DeletedAt { get; set; }

        public int? DefaultClinicId { get; set; }

        // Navegaciones
        public virtual Clinic DefaultClinic { get; set; }
        public virtual ICollection<UserClinic> UserClinics { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<UserTask> UserTasks { get; set; }
        public virtual Settings Settings { get; set; }

        // Relación con MedicalHistory (auditoría: quién creó/modificó registros)
        public virtual ICollection<MedicalHistory> CreatedMedicalHistories { get; set; }
        public virtual ICollection<MedicalHistory> ModifiedMedicalHistories { get; set; }
    }
}