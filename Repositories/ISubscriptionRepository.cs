using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface ISubscriptionRepository
    {
        Task<List<SubscriptionPlan>> GetAllPlansAsync();
        Task<Subscription?> GetActiveByAccountIdAsync(int accountId);
        Task<SubscriptionPlan?> GetPlanByIdAsync(int planId);
        Task<SubscriptionPlan?> GetPlanByNameAsync(string name);
        Task<Subscription> CreateAsync(Subscription subscription);
        Task UpdateAsync(Subscription subscription);
        Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId);
        Task<SubscriptionPlan?> GetPlanByStripePriceIdAsync(string stripePriceId);
        Task<int> GetTeamMemberCountAsync(int accountId);
        Task<int> GetClinicCountAsync(int accountId);
    }
}
