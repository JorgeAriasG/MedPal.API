using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IFoodItemRepository
    {
        Task<IEnumerable<FoodItem>> GetAllAsync();
        Task<IEnumerable<FoodItem>> SearchAsync(string query);
        Task<IEnumerable<FoodItem>> GetByCategoryAsync(string category);
        Task<FoodItem?> GetByIdAsync(int id);
        Task<FoodItem> AddAsync(FoodItem foodItem);
        void Update(FoodItem foodItem);
        void Remove(FoodItem foodItem);
        Task<bool> ExistsByNameAsync(string name);
        Task<IEnumerable<string>> GetAllCategoriesAsync();
        Task<int> CompleteAsync();
    }
}
