using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    public class BookingCompleteDTO
    {
        public string? Sr { get; set; }

        public int? ClinicId { get; set; }

        public int? DoctorId { get; set; }

        public string? PatientName { get; set; }

        public string? PatientPhone { get; set; }

        [Required]
        public DateOnly Date { get; set; }

        [Required]
        public TimeOnly Time { get; set; }

        public int DurationMinutes { get; set; } = 30;

        [Required]
        public bool ConsentMedicalRecords { get; set; }

        [Required]
        public bool ConsentWhatsapp { get; set; }
    }
}