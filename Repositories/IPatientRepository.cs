using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.DTOs;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patient>> GetAllPatientsAsync(int clinicId, int? userId = null, string? search = null, string? sortBy = "name", bool descending = false);
        Task<Patient?> GetPatientByIdAsync(int id);
        Task<Patient> AddPatientAsync(Patient patient);
        Task UpdatePatientAsync(int id, Patient patient);
        Task DeletePatientAsync(int id);

        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

        Task<IEnumerable<string>> GetPatientAllergyNamesAsync(int patientId, CancellationToken cancellationToken);

        Task AddPatientClinicsAsync(int patientId, List<int> clinicIds);
        Task SyncPatientClinicsAsync(int patientId, List<int> newClinicIds);
        Task<bool> UserBelongsToClinicAsync(int userId, int clinicId);
        Task<int?> GetClinicAccountIdAsync(int clinicId);
        Task CreatePatientAccountAsync(int patientId, int accountId, bool isPrimary, bool isVerifiedByPatient, bool? consentToShareProfile);
        Task<bool> HasVerifiedMembershipAsync(int patientId, int accountId);
    }
}
