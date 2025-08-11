using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedPal.API.Models
{
    public class MedicalHistory
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("PatientDetails")]
        public int PatientDetailsId { get; set; }

        [Required]
        public string ConditionName { get; set; }

        [Required]
        public DateTime DiagnosisDate { get; set; }

        [Required]
        public string Treatment { get; set; }

        [Required]
        public string Medications { get; set; }

        public string DoctorNotes { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public virtual PatientDetails PatientDetails { get; set; }
    }
}