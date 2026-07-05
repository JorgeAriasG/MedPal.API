using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.DTOs;

namespace MedPal.API.Services
{
    public interface ISubscriptionService
    {
        Task<List<SubscriptionPlanReadDTO>> GetPlansAsync();
        Task<SubscriptionReadDTO?> GetCurrentSubscriptionAsync(int accountId);
        Task<bool> CanAddUserAsync(int accountId);
        Task<bool> CanAddClinicAsync(int accountId);
        Task<string> GetLimitExceededMessageAsync(int accountId, string resourceType);
        Task<SubscriptionStatusDTO> GetSubscriptionStatusAsync(int accountId);
        Task<CheckoutSessionResponse> CreatePendingSubscriptionAsync(int accountId, string planName, string? stripeCustomerId = null);
        Task AssignPlanAsync(int accountId, string planName = "SOLO");
    }
}
