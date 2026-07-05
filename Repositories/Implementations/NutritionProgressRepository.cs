using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class NutritionProgressRepository : TenantAwareRepository<NutritionProgress>, INutritionProgressRepository
    {
        public NutritionProgressRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<NutritionProgress>> GetByPatientDetailsIdAsync(int patientDetailsId)
        {
            return await ApplyTenantFilter(_context.NutritionProgresses
                .Include(np => np.PatientDetails)
                .Where(np => np.PatientDetailsId == patientDetailsId && !np.IsDeleted))
                .OrderByDescending(np => np.RecordedAt)
                .ToListAsync();
        }

        public async Task<NutritionProgress?> GetByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.NutritionProgresses
                .Include(np => np.PatientDetails))
                .FirstOrDefaultAsync(np => np.Id == id && !np.IsDeleted);
        }

        public async Task<NutritionProgress> AddAsync(NutritionProgress progress)
        {
            await _context.NutritionProgresses.AddAsync(progress);
            return progress;
        }

        public void Update(NutritionProgress progress)
        {
            _context.NutritionProgresses.Update(progress);
        }

        public void Remove(NutritionProgress progress)
        {
            _context.NutritionProgresses.Remove(progress);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
