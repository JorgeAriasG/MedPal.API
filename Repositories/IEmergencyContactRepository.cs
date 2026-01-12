using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IEmergencyContactRepository
    {
        Task<IEnumerable<EmergencyContact>> GetAllEmergencyContactsAsync();
        Task<EmergencyContact> GetEmergencyContactByIdAsync(int id);
        Task<IEnumerable<EmergencyContact>> GetEmergencyContactsByPatientIdAsync(int patientId);
        Task<IEnumerable<EmergencyContact>> GetActiveEmergencyContactsByPatientIdAsync(int patientId);
        Task<EmergencyContact> AddEmergencyContactAsync(EmergencyContact emergencyContact);
        void UpdateEmergencyContact(EmergencyContact emergencyContact);
        void RemoveEmergencyContact(EmergencyContact emergencyContact);
        Task<int> CompleteAsync();
    }
}
