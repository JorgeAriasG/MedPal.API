using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedPal.API.Data;
using MedPal.API.Models;

namespace MedPal.API.Repositories.Implementations
{
    public class PendingRegistrationRepository : Repository<PendingRegistration>, IPendingRegistrationRepository
    {
        public PendingRegistrationRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<PendingRegistration> CreateAsync(PendingRegistration registration)
        {
            await _dbSet.AddAsync(registration);
            await _context.SaveChangesAsync();
            return registration;
        }

        public async Task<PendingRegistration?> GetBySessionIdAsync(string sessionId)
        {
            return await _dbSet
                .FirstOrDefaultAsync(r => r.StripeSessionId == sessionId);
        }

        public async Task UpdateStatusAsync(int id, string status, int? accountId = null)
        {
            var entity = await _dbSet.FindAsync(id);
            if (entity != null)
            {
                entity.Status = status;
                entity.UpdatedAt = DateTime.UtcNow;
                if (accountId.HasValue)
                    entity.AccountId = accountId.Value;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteExpiredAsync()
        {
            var expired = await _dbSet
                .Where(r => r.Status == "pending" && r.ExpiresAt <= DateTime.UtcNow)
                .ToListAsync();
            _dbSet.RemoveRange(expired);
            await _context.SaveChangesAsync();
        }
    }
}
