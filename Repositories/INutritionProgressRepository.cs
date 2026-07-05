using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface INutritionProgressRepository
    {
        Task<IEnumerable<NutritionProgress>> GetByPatientDetailsIdAsync(int patientDetailsId);
        Task<NutritionProgress?> GetByIdAsync(int id);
        Task<NutritionProgress> AddAsync(NutritionProgress progress);
        void Update(NutritionProgress progress);
        void Remove(NutritionProgress progress);
        Task<int> CompleteAsync();
    }
}
