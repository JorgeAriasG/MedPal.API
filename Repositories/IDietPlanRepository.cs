using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IDietPlanRepository
    {
        Task<IEnumerable<DietPlan>> GetByPatientDetailsIdAsync(int patientDetailsId);
        Task<DietPlan?> GetByIdAsync(int id);
        Task<DietPlan?> GetWithMealsAsync(int id);
        Task<DietPlan> AddAsync(DietPlan dietPlan);
        void Update(DietPlan dietPlan);
        void Remove(DietPlan dietPlan);
        Task<int> CompleteAsync();
    }
}
