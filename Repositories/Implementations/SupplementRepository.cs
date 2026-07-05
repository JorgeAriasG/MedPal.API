using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class SupplementRepository : TenantAwareRepository<Supplement>, ISupplementRepository
    {
        public SupplementRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<Supplement>> GetByPatientDetailsIdAsync(int patientDetailsId)
        {
            return await ApplyTenantFilter(_context.Supplements
                .Include(s => s.PatientDetails)
                .Where(s => s.PatientDetailsId == patientDetailsId && !s.IsDeleted))
                .OrderByDescending(s => s.IsActive)
                .ThenBy(s => s.Name)
                .ToListAsync();
        }

        public async Task<Supplement?> GetByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.Supplements
                .Include(s => s.PatientDetails))
                .FirstOrDefaultAsync(s => s.Id == id && !s.IsDeleted);
        }

        public async Task<Supplement> AddAsync(Supplement supplement)
        {
            await _context.Supplements.AddAsync(supplement);
            return supplement;
        }

        public void Update(Supplement supplement)
        {
            _context.Supplements.Update(supplement);
        }

        public void Remove(Supplement supplement)
        {
            _context.Supplements.Remove(supplement);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
