using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class PatientInsurance : ISoftDelete
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

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        public virtual Patient Patient { get; set; }
        public virtual InsuranceProvider InsuranceProvider { get; set; }
    }
}