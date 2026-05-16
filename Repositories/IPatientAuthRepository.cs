using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IPatientAuthRepository
    {
        Task<PatientAuth> GetByEmailAsync(string email);
        Task<PatientAuth> GetByPatientIdAsync(int patientId);
        Task<bool> EmailExistsAsync(string email);
        Task<PatientAuth> CreateAsync(PatientAuth patientAuth);
        Task UpdateLastLoginAsync(int patientAuthId);
    }
}
