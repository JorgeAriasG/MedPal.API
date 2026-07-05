using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class SupplementWriteDTO
    {
        [Required(ErrorMessage = "El ID de PatientDetails es requerido")]
        public int PatientDetailsId { get; set; }

        [Required(ErrorMessage = "El nombre del suplemento es requerido")]
        [StringLength(200, ErrorMessage = "El nombre no puede exceder 200 caracteres")]
        public string Name { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Brand { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Form { get; set; }

        [StringLength(100)]
        public string? Dosage { get; set; }

        [StringLength(50)]
        public string? Frequency { get; set; }

        [StringLength(50)]
        public string? Unit { get; set; }

        [StringLength(50)]
        public string? Timing { get; set; }

        [StringLength(100)]
        public string? Duration { get; set; }

        [StringLength(500)]
        public string? Indication { get; set; }

        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? PrescribedAt { get; set; }

        public int? PrescribedById { get; set; }

        [StringLength(500)]
        public string? Notes { get; set; }
    }
}
