using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.Models
{
    public class Patient
    {
        public Patient()
        {
            Appointments = new HashSet<Appointment>();
            Reports = new HashSet<Report>();
            PatientsInsurance = new HashSet<PatientInsurance>();
            Invoices = new HashSet<Invoice>();
            UserTasks = new HashSet<UserTask>();
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
        [Required]
        public string EmergencyContact { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        [Required]
        public DateTime UpdatedAt { get; set; }
        public int ClinicId { get; set; }
        // Relación opcional con User (para portal de pacientes)
        public int? UserId { get; set; }

        // Navigation Properties
        public virtual Clinic Clinic { get; set; }
        public virtual User User { get; set; }
        public virtual PatientDetails PatientDetails { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<Report> Reports { get; set; }
        public virtual ICollection<PatientInsurance> PatientsInsurance { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
        public virtual ICollection<UserTask> UserTasks { get; set; }
    }
}