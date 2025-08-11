using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IPatientDetailsRepository
    {
        Task<IEnumerable<PatientDetails>> GetAllPatientDetailsAsync();
        Task<PatientDetails> GetPatientDetailsByIdAsync(int id);
        Task<PatientDetails> AddPatientDetailsAsync(PatientDetails patientDetails);
        void UpdatePatientDetails(PatientDetails patientDetails);
        void RemovePatientDetails(PatientDetails patientDetails);
        Task<int> CompleteAsync();
    }
}
