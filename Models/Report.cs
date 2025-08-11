using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedPal.API.Models
{
    public class Report
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Patient")]
        [Required]
        public int PatientId { get; set; }

        [Required]
        public string ReportType { get; set; }

        [Required]
        public string ReportFile { get; set; }

        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public virtual Patient Patient { get; set; }
    }
}