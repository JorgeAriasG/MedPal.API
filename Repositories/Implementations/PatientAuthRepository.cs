using MedPal.API.Data;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class PatientAuthRepository : IPatientAuthRepository
    {
        private readonly AppDbContext _context;

        public PatientAuthRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PatientAuth> GetByEmailAsync(string email)
        {
            return await _context.Set<PatientAuth>()
                .Include(pa => pa.Patient)
                .FirstOrDefaultAsync(pa => pa.Email.ToLower() == email.ToLower());
        }

        public async Task<PatientAuth> GetByPatientIdAsync(int patientId)
        {
            return await _context.Set<PatientAuth>()
                .Include(pa => pa.Patient)
                .FirstOrDefaultAsync(pa => pa.PatientId == patientId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _context.Set<PatientAuth>()
                .AnyAsync(pa => pa.Email.ToLower() == email.ToLower());
        }

        public async Task<PatientAuth> CreateAsync(PatientAuth patientAuth)
        {
            await _context.Set<PatientAuth>().AddAsync(patientAuth);
            await _context.SaveChangesAsync();
            return patientAuth;
        }

        public async Task UpdateLastLoginAsync(int patientAuthId)
        {
            var auth = await _context.Set<PatientAuth>().FindAsync(patientAuthId);
            if (auth != null)
            {
                auth.LastLoginAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
