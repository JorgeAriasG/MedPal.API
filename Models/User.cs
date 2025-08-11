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
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }
        public int? DefaultClinicId { get; set; }

        public virtual Clinic DefaultClinic { get; set; }
        public virtual ICollection<UserClinic> UserClinics { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
        public virtual ICollection<UserTask> UserTasks { get; set; }
        public virtual Settings Settings { get; set; }
    }
}