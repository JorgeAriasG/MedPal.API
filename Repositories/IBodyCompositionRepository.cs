using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IBodyCompositionRepository
    {
        Task<IEnumerable<BodyComposition>> GetByPatientDetailsIdAsync(int patientDetailsId);
        Task<BodyComposition?> GetByIdAsync(int id);
        Task<BodyComposition?> GetLatestAsync(int patientDetailsId);
        Task<BodyComposition> AddAsync(BodyComposition bodyComposition);
        void Update(BodyComposition bodyComposition);
        void Remove(BodyComposition bodyComposition);
        Task<int> CompleteAsync();
    }
}
