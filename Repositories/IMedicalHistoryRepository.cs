using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IMedicalHistoryRepository
    {
        Task<IEnumerable<MedicalHistory>> GetAllMedicalHistoriesAsync();
        Task<MedicalHistory> GetMedicalHistoryByIdAsync(int id);
        Task<MedicalHistory> AddMedicalHistoryAsync(MedicalHistory medicalHistory);
        void UpdateMedicalHistory(MedicalHistory medicalHistory);
        void RemoveMedicalHistory(MedicalHistory medicalHistory);
        Task<int> CompleteAsync();
    }
}