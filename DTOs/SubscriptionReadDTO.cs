using System;

namespace MedPal.API.DTOs
{
    public class SubscriptionReadDTO
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int SubscriptionPlanId { get; set; }
        public string Status { get; set; }
        public DateTime CurrentPeriodStart { get; set; }
        public DateTime CurrentPeriodEnd { get; set; }
        public DateTime? TrialEndsAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public int MaxTeamMembers { get; set; }
        public int MaxClinics { get; set; }
        public int MaxActiveCalendars { get; set; }
        public int CurrentTeamMembers { get; set; }
        public int CurrentClinics { get; set; }
        public SubscriptionPlanReadDTO Plan { get; set; }
    }
}
