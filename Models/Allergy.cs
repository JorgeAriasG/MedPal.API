using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class Allergy : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PatientDetails")]
        public int PatientDetailsId { get; set; }

        [Required]
        public string AllergyName { get; set; }

        [Required]
        public string Severity { get; set; }

        public string Notes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        public virtual PatientDetails PatientDetails { get; set; }
    }
}