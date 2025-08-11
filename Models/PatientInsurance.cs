using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedPal.API.Models
{
    public class PatientInsurance
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Patient")]
        public int PatientId { get; set; }

        [ForeignKey("InsuranceProvider")]
        public int InsuranceProviderId { get; set; }

        [Required]
        public string PlanNumber { get; set; }

        [Required]
        public DateTime CoverageStartDate { get; set; }

        [Required]
        public DateTime CoverageEndDate { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; }

        [Required]
        public DateTime UpdatedAt { get; set; }

        public virtual Patient Patient { get; set; }
        public virtual InsuranceProvider InsuranceProvider { get; set; }
    }
}