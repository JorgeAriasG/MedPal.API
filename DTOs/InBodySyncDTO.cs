using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class InBodySyncDTO
    {
        [Required(ErrorMessage = "El ID de PatientDetails es requerido")]
        public int PatientDetailsId { get; set; }

        [Required(ErrorMessage = "Los datos crudos de InBody son requeridos")]
        public string RawData { get; set; } = string.Empty;

        public DateTime? RecordedAt { get; set; }
    }

    public class InBodySyncResultDTO
    {
        public bool Success { get; set; }
        public int? BodyCompositionId { get; set; }
        public string? Message { get; set; }
        public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
    }
}
