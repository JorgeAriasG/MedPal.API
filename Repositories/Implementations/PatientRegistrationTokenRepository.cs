using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedPal.API.Data;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class PatientRegistrationTokenRepository : IPatientRegistrationTokenRepository
    {
        private readonly AppDbContext _context;

        public PatientRegistrationTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public Task<PatientRegistrationToken> CreateAsync(PatientRegistrationToken token)
        {
            _context.PatientRegistrationTokens.Add(token);
            return Task.FromResult(token);
        }

        public async Task<PatientRegistrationToken?> GetByHashAsync(string tokenHash)
        {
            return await _context.PatientRegistrationTokens
                .AsNoTracking()
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        }

        public async Task<IEnumerable<PatientRegistrationToken>> GetPendingByPatientIdAsync(int patientId)
        {
            var now = DateTime.UtcNow;
            return await _context.PatientRegistrationTokens
                .AsNoTracking()
                .IgnoreQueryFilters()
                .Where(t => t.PatientId == patientId && t.Status == "pending" && t.ExpiresAt > now)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public Task UpdateAsync(PatientRegistrationToken token)
        {
            _context.PatientRegistrationTokens.Update(token);
            return Task.CompletedTask;
        }

        public Task<int> ConsumeAsync(string tokenHash)
        {
            return _context.PatientRegistrationTokens
                .IgnoreQueryFilters()
                .Where(t => t.TokenHash == tokenHash && t.Status == "pending")
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.Status, "used")
                    .SetProperty(t => t.UsedAt, DateTime.UtcNow));
        }

        public Task<int> RevokeAsync(string tokenHash)
        {
            return _context.PatientRegistrationTokens
                .IgnoreQueryFilters()
                .Where(t => t.TokenHash == tokenHash && t.Status == "pending")
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.Status, "revoked"));
        }

        public Task<int> RevokePendingByPatientAsync(int patientId)
        {
            return _context.PatientRegistrationTokens
                .IgnoreQueryFilters()
                .Where(t => t.PatientId == patientId && t.Status == "pending")
                .ExecuteUpdateAsync(s => s
                    .SetProperty(t => t.Status, "revoked"));
        }
    }
}