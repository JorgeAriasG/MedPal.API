using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IAnthropometryRepository
    {
        Task<IEnumerable<AnthropometryRecord>> GetByPatientDetailsIdAsync(int patientDetailsId);
        Task<AnthropometryRecord?> GetByIdAsync(int id);
        Task<AnthropometryRecord> AddAsync(AnthropometryRecord record);
        void Update(AnthropometryRecord record);
        void Remove(AnthropometryRecord record);
        Task<int> CompleteAsync();
    }
}
