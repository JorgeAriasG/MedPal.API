using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class FoodItemRepository : TenantAwareRepository<FoodItem>, IFoodItemRepository
    {
        public FoodItemRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<FoodItem>> GetAllAsync()
        {
            return await ApplyTenantFilter(_context.FoodItems
                .Where(fi => !fi.IsDeleted && fi.IsActive))
                .OrderBy(fi => fi.Category)
                .ThenBy(fi => fi.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<FoodItem>> SearchAsync(string query)
        {
            var results = await GetAllAsync();
            return results.Where(fi =>
                fi.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (fi.Brand != null && fi.Brand.Contains(query, StringComparison.OrdinalIgnoreCase)) ||
                fi.Category.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        public async Task<IEnumerable<FoodItem>> GetByCategoryAsync(string category)
        {
            return await ApplyTenantFilter(_context.FoodItems
                .Where(fi => !fi.IsDeleted && fi.IsActive && fi.Category == category))
                .OrderBy(fi => fi.Name)
                .ToListAsync();
        }

        public async Task<FoodItem?> GetByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.FoodItems)
                .FirstOrDefaultAsync(fi => fi.Id == id && !fi.IsDeleted);
        }

        public async Task<FoodItem> AddAsync(FoodItem foodItem)
        {
            await _context.FoodItems.AddAsync(foodItem);
            return foodItem;
        }

        public void Update(FoodItem foodItem)
        {
            _context.FoodItems.Update(foodItem);
        }

        public void Remove(FoodItem foodItem)
        {
            _context.FoodItems.Remove(foodItem);
        }

        public async Task<bool> ExistsByNameAsync(string name)
        {
            return await ApplyTenantFilter(_context.FoodItems)
                .AnyAsync(fi => fi.Name.ToLower() == name.ToLower() && !fi.IsDeleted);
        }

        public async Task<IEnumerable<string>> GetAllCategoriesAsync()
        {
            return await ApplyTenantFilter(_context.FoodItems
                .Where(fi => !fi.IsDeleted && fi.IsActive))
                .Select(fi => fi.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
