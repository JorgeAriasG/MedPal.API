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

        public async Task<PatientRegistrationToken> CreateAsync(PatientRegistrationToken token)
        {
            _context.PatientRegistrationTokens.Add(token);
            await _context.SaveChangesAsync();
            return token;
        }

        public async Task<PatientRegistrationToken?> GetByHashAsync(string tokenHash)
        {
            return await _context.PatientRegistrationTokens
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
        }

        public async Task<IEnumerable<PatientRegistrationToken>> GetPendingByPatientIdAsync(int patientId)
        {
            var now = DateTime.UtcNow;
            return await _context.PatientRegistrationTokens
                .IgnoreQueryFilters()
                .Where(t => t.PatientId == patientId && t.Status == "pending" && t.ExpiresAt > now)
                .OrderByDescending(t => t.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(PatientRegistrationToken token)
        {
            _context.PatientRegistrationTokens.Update(token);
            await _context.SaveChangesAsync();
        }
    }
}