using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.Models
{
    public class SubscriptionPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public int MaxTeamMembers { get; set; }

        [Required]
        public int MaxClinics { get; set; }

        [Required]
        public int MaxActiveCalendars { get; set; }

        [Required]
        public int TrialDays { get; set; } = 0;

        [StringLength(100)]
        public string? StripePriceId { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
