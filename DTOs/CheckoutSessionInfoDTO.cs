using System;

namespace MedPal.API.DTOs
{
    public class CheckoutSessionInfoDTO
    {
        public string PlanName { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string? CustomerEmail { get; set; }
        public DateTime? TrialEnd { get; set; }
    }
}
