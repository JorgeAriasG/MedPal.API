using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class BodyCompositionRepository : TenantAwareRepository<BodyComposition>, IBodyCompositionRepository
    {
        public BodyCompositionRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<BodyComposition>> GetByPatientDetailsIdAsync(int patientDetailsId)
        {
            return await ApplyTenantFilter(_context.BodyCompositions
                .Include(bc => bc.PatientDetails)
                .Where(bc => bc.PatientDetailsId == patientDetailsId && !bc.IsDeleted))
                .OrderByDescending(bc => bc.RecordedAt)
                .ToListAsync();
        }

        public async Task<BodyComposition?> GetByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.BodyCompositions
                .Include(bc => bc.PatientDetails))
                .FirstOrDefaultAsync(bc => bc.Id == id && !bc.IsDeleted);
        }

        public async Task<BodyComposition?> GetLatestAsync(int patientDetailsId)
        {
            return await ApplyTenantFilter(_context.BodyCompositions
                .Where(bc => bc.PatientDetailsId == patientDetailsId && !bc.IsDeleted))
                .OrderByDescending(bc => bc.RecordedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<BodyComposition> AddAsync(BodyComposition bodyComposition)
        {
            await _context.BodyCompositions.AddAsync(bodyComposition);
            return bodyComposition;
        }

        public void Update(BodyComposition bodyComposition)
        {
            _context.BodyCompositions.Update(bodyComposition);
        }

        public void Remove(BodyComposition bodyComposition)
        {
            _context.BodyCompositions.Remove(bodyComposition);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
