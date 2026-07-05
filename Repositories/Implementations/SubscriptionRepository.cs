using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedPal.API.Data;
using MedPal.API.Models;

namespace MedPal.API.Repositories.Implementations
{
    public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
    {
        public SubscriptionRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<List<SubscriptionPlan>> GetAllPlansAsync()
        {
            return await _context.Set<SubscriptionPlan>()
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        public async Task<Subscription?> GetActiveByAccountIdAsync(int accountId)
        {
            return await _dbSet
                .Include(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(s => s.AccountId == accountId && s.IsActive);
        }

        public async Task<SubscriptionPlan?> GetPlanByIdAsync(int planId)
        {
            return await _context.Set<SubscriptionPlan>().FindAsync(planId);
        }

        public async Task<SubscriptionPlan?> GetPlanByNameAsync(string name)
        {
            return await _context.Set<SubscriptionPlan>()
                .FirstOrDefaultAsync(p => p.Name == name && p.IsActive);
        }

        public async Task<Subscription> CreateAsync(Subscription subscription)
        {
            await _dbSet.AddAsync(subscription);
            await _context.SaveChangesAsync();
            return subscription;
        }

        public async Task UpdateAsync(Subscription subscription)
        {
            _dbSet.Update(subscription);
            await _context.SaveChangesAsync();
        }

        public async Task<Subscription?> GetByStripeSubscriptionIdAsync(string stripeSubscriptionId)
        {
            return await _dbSet
                .Include(s => s.SubscriptionPlan)
                .FirstOrDefaultAsync(s => s.StripeSubscriptionId == stripeSubscriptionId);
        }

        public async Task<SubscriptionPlan?> GetPlanByStripePriceIdAsync(string stripePriceId)
        {
            return await _context.Set<SubscriptionPlan>()
                .FirstOrDefaultAsync(p => p.StripePriceId == stripePriceId && p.IsActive);
        }

        public async Task<int> GetTeamMemberCountAsync(int accountId)
        {
            return await _context.Users
                .CountAsync(u => u.AccountId == accountId && !u.IsDeleted);
        }

        public async Task<int> GetClinicCountAsync(int accountId)
        {
            return await _context.Clinics
                .CountAsync(c => c.AccountId == accountId && !c.IsDeleted);
        }
    }
}
