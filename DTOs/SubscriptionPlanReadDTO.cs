namespace MedPal.API.DTOs
{
    public class SubscriptionPlanReadDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int MaxTeamMembers { get; set; }
        public int MaxClinics { get; set; }
        public int MaxActiveCalendars { get; set; }
        public int TrialDays { get; set; }
        public string? StripePriceId { get; set; }
    }
}
