using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class MedicalHistoryWriteDTO
    {
        [Required]
        public int PatientDetailsId { get; set; }

        [Required]
        public string SpecialtyType { get; set; }

        [Required]
        public string Diagnosis { get; set; }

        [Required]
        public DateTime DiagnosisDate { get; set; }

        [Required]
        public string TreatmentPlan { get; set; }

        public string PrescribedMedications { get; set; }

        public string ClinicalNotes { get; set; }

        public string TreatmentStatus { get; set; } = "Activo";

        public DateTime? FollowUpDate { get; set; }

        public bool IsConfidential { get; set; } = true;
    }
}