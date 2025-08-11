using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.Models
{
    public class Clinic
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

        public virtual ICollection<UserClinic> UserClinics { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}