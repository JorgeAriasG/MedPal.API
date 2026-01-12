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

        public string ClinicalNotes { get; set; }

        public DateTime? FollowUpDate { get; set; }

        public string SpecialtyData { get; set; }

        // Relación opcional con una Prescription
        public int? PrescriptionId { get; set; }

        public bool IsConfidential { get; set; } = true;
    }
}