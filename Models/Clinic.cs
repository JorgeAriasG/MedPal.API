using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class Clinic : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public string ContactInfo { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public TimeOnly Open { get; set; }
        
        public TimeOnly Close { get; set; }

        // Multi-tenancy: Account relationship (Fase 1)
        [ForeignKey("Account")]
        public int? AccountId { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        // Navigations
        public virtual Account Account { get; set; }

        public virtual ICollection<UserClinic> UserClinics { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}