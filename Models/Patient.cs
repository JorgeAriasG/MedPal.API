using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class Patient : ISoftDelete, IAuditableEntity
    {
        public Patient()
        {
            Appointments = new HashSet<Appointment>();
            Reports = new HashSet<Report>();
            PatientsInsurance = new HashSet<PatientInsurance>();
            Invoices = new HashSet<Invoice>();
            EmergencyContacts = new HashSet<EmergencyContact>();
        }

        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Middlename { get; set; }
        [Required]
        public string Lastname { get; set; }
        [Required]
        public DateTime Dob { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public string Address { get; set; }
        public string Phone { get; set; }
        [Required]
        public string Email { get; set; }

        // REMOVIDO EN PHASE 3: EmergencyContact string
        // Se reemplaza con relación 1:Many a EmergencyContact model para más flexibilidad

        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime? UpdatedAt { get; set; }
        public int ClinicId { get; set; }
        // Relación opcional con User (para portal de pacientes)
        public int? UserId { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        // IAuditableEntity implementation
        public int? CreatedByUserId { get; set; }
        public int? UpdatedByUserId { get; set; }
        public DateTime? LastModifiedAt { get; set; }
        public int? LastModifiedByUserId { get; set; }
        public bool IsAnonymized { get; set; } = false;
        public bool IsMarketingBlocked { get; set; } = false;

        // Navigation Properties
        public virtual Clinic Clinic { get; set; }
        public virtual User User { get; set; }
        public virtual PatientDetails PatientDetails { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<PatientInsurance> PatientsInsurance { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
        
        /// <summary>
        /// Contactos de emergencia del paciente (puede tener múltiples)
        /// </summary>
        public virtual ICollection<EmergencyContact> EmergencyContacts { get; set; }
    }
}