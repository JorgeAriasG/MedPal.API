using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IPatientRegistrationTokenRepository
    {
        Task<PatientRegistrationToken> CreateAsync(PatientRegistrationToken token);
        Task<PatientRegistrationToken?> GetByHashAsync(string tokenHash);
        Task<IEnumerable<PatientRegistrationToken>> GetPendingByPatientIdAsync(int patientId);
        Task UpdateAsync(PatientRegistrationToken token);
    }
}