using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedPal.API.Models
{
    public class Subscription
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Account")]
        [Required]
        public int AccountId { get; set; }

        [ForeignKey("SubscriptionPlan")]
        [Required]
        public int SubscriptionPlanId { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Active";

        [Required]
        public DateTime CurrentPeriodStart { get; set; }

        [Required]
        public DateTime CurrentPeriodEnd { get; set; }

        public DateTime? TrialEndsAt { get; set; }
        public DateTime? CancelledAt { get; set; }

        [StringLength(100)]
        public string? StripeCustomerId { get; set; }

        [StringLength(100)]
        public string? StripeSubscriptionId { get; set; }

        [Required]
        public int MaxTeamMembers { get; set; }

        [Required]
        public int MaxClinics { get; set; }

        [Required]
        public int MaxActiveCalendars { get; set; }

        [Required]
        public bool IsActive { get; set; } = true;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual Account Account { get; set; }
        public virtual SubscriptionPlan SubscriptionPlan { get; set; }
    }
}
