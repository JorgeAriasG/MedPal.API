using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using MedPal.API.Models;

namespace MedPal.API.DTOs
{
    public class PrescriptionItemDTO
    {
        [Required]
        public string MedicationName { get; set; }
        [Required]
        public string Dosage { get; set; }
        [Required]
        public string Frequency { get; set; }
        [Required]
        public string Duration { get; set; }
        public string Instructions { get; set; }
    }

    public class PrescriptionWriteDTO
    {
        [Required]
        public int PatientId { get; set; }
        [Required]
        public string Diagnosis { get; set; }
        public string Notes { get; set; }
        [Required]
        public DateTime ExpiresAt { get; set; }
        [Required]
        public List<PrescriptionItemDTO> Items { get; set; }
        // Relación opcional con MedicalHistory
        public int? MedicalHistoryId { get; set; }
    }

    public class PrescriptionReadDTO
    {
        public int Id { get; set; }
        public Guid UniqueCode { get; set; }
        public string DoctorName { get; set; }
        public string DoctorSpecialty { get; set; }
        public string DoctorLicense { get; set; }
        public string PatientName { get; set; }
        public string Diagnosis { get; set; }
        public string Notes { get; set; }
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public string Status { get; set; }
        public int? MedicalHistoryId { get; set; }
        public IEnumerable<PrescriptionItemDTO> Items { get; set; }
    }

    public class PrescriptionValidationDTO
    {
        public bool IsValid { get; set; }
        public string Status { get; set; }
        public string DoctorName { get; set; } // Minimal info
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public IEnumerable<PrescriptionItemDTO> Items { get; set; } // Show meds to pharmacist
    }
}
