using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.DTOs;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IMedicalHistoryRepository
    {
        Task<IEnumerable<MedicalHistory>> GetAllMedicalHistoriesAsync();
        Task<MedicalHistory> GetMedicalHistoryByIdAsync(int id);
        Task<MedicalHistory> AddMedicalHistoryAsync(MedicalHistory medicalHistory);
        Task<IEnumerable<MedicalHistory>> GetMedicalHistoriesByPatientIdAsync(int patientId);
        Task<IEnumerable<MedicalHistorySummaryReadDTO>> GetRecentHistoriesByPatientDetailsIdAsync(int patientDetailsId, int take);
        void UpdateMedicalHistory(MedicalHistory medicalHistory);
        void RemoveMedicalHistory(MedicalHistory medicalHistory);
        Task<int> CompleteAsync();
    }
}