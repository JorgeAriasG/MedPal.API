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

        public string Specialty { get; set; }

        public string ProfessionalLicenseNumber { get; set; }

        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; } = false;
        public DateTime? LastAccessAt { get; set; }

        public bool HasAcceptedPrivacyTerms { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public int? DeactivatedByUserId { get; set; }

        public DateTime? DeletedAt { get; set; }

        // Navigations
        public virtual ICollection<UserClinic> UserClinics { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<UserTask> UserTasks { get; set; }
        public virtual Settings Settings { get; set; }

        // Authorization relationships
        public virtual ICollection<Authorization.UserRole> UserRoles { get; set; } = new List<Authorization.UserRole>();

        // Audit relationships
        public virtual ICollection<MedicalHistory> CreatedMedicalHistories { get; set; }
        public virtual ICollection<MedicalHistory> ModifiedMedicalHistories { get; set; }
    }
}