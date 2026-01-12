using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class InsuranceProvider : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string ProviderType { get; set; }

        [Required]
        public string ContactInfo { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        public virtual ICollection<PatientInsurance> PatientsInsurance { get; set; }
    }
}