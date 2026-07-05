using System;

namespace MedPal.API.DTOs
{
    public class SupplementReadDTO
    {
        public int Id { get; set; }
        public int PatientDetailsId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Description { get; set; }
        public string? Form { get; set; }
        public string? Dosage { get; set; }
        public string? Frequency { get; set; }
        public string? Unit { get; set; }
        public string? Timing { get; set; }
        public string? Duration { get; set; }
        public string? Indication { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime? PrescribedAt { get; set; }
        public int? PrescribedById { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
