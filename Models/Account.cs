using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.Models
{
    /// <summary>
    /// Representa una cuenta/organización en el sistema.
    /// Una cuenta puede contener múltiples clínicas, usuarios y pacientes.
    /// </summary>
    public class Account
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(255)]
        public string Name { get; set; }

        [StringLength(1000)]
        public string Description { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navegaciones
        public virtual ICollection<Clinic> Clinics { get; set; } = new List<Clinic>();
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<PatientAccount> PatientAccounts { get; set; } = new List<PatientAccount>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
    }
}
