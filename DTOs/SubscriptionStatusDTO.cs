namespace MedPal.API.DTOs
{
    public class SubscriptionStatusDTO
    {
        public string Status { get; set; }
        public bool HasAccess { get; set; }
        public string? PlanName { get; set; }
        public bool IsTrialing { get; set; }
        public DateTime? TrialEndsAt { get; set; }
    }
}
