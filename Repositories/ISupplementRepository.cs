using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface ISupplementRepository
    {
        Task<IEnumerable<Supplement>> GetByPatientDetailsIdAsync(int patientDetailsId);
        Task<Supplement?> GetByIdAsync(int id);
        Task<Supplement> AddAsync(Supplement supplement);
        void Update(Supplement supplement);
        void Remove(Supplement supplement);
        Task<int> CompleteAsync();
    }
}
