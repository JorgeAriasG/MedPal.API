using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IVitalSignRepository
    {
        Task<IEnumerable<VitalSign>> GetAllVitalSignsAsync();
        Task<VitalSign?> GetVitalSignByIdAsync(int id);
        Task<IEnumerable<VitalSign>> GetVitalSignsByPatientDetailsIdAsync(int patientDetailsId);
        Task<VitalSign> AddVitalSignAsync(VitalSign vitalSign);
        void UpdateVitalSign(VitalSign vitalSign);
        void RemoveVitalSign(VitalSign vitalSign);
        Task<int> CompleteAsync();
    }
}
