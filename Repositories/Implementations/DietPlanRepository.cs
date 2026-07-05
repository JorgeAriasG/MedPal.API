using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class DietPlanRepository : TenantAwareRepository<DietPlan>, IDietPlanRepository
    {
        public DietPlanRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<DietPlan>> GetByPatientDetailsIdAsync(int patientDetailsId)
        {
            return await ApplyTenantFilter(_context.DietPlans
                .Include(dp => dp.Meals)
                    .ThenInclude(m => m.Items)
                    .ThenInclude(i => i.FoodItem)
                .Where(dp => dp.PatientDetailsId == patientDetailsId && !dp.IsDeleted))
                .OrderByDescending(dp => dp.CreatedAt)
                .ToListAsync();
        }

        public async Task<DietPlan?> GetByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.DietPlans)
                .FirstOrDefaultAsync(dp => dp.Id == id && !dp.IsDeleted);
        }

        public async Task<DietPlan?> GetWithMealsAsync(int id)
        {
            return await ApplyTenantFilter(_context.DietPlans
                .Include(dp => dp.Meals)
                    .ThenInclude(m => m.Items)
                    .ThenInclude(i => i.FoodItem))
                .FirstOrDefaultAsync(dp => dp.Id == id && !dp.IsDeleted);
        }

        public async Task<DietPlan> AddAsync(DietPlan dietPlan)
        {
            await _context.DietPlans.AddAsync(dietPlan);
            return dietPlan;
        }

        public void Update(DietPlan dietPlan)
        {
            _context.DietPlans.Update(dietPlan);
        }

        public void Remove(DietPlan dietPlan)
        {
            _context.DietPlans.Remove(dietPlan);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
