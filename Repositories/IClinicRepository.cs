using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IClinicRepository
    {
        Task<Clinic> GetClinicByIdAsync(int id);
        Task<Clinic> AddClinicAsync(int userId, Clinic clinic);
        void DetachEntity<T>(T entity) where T : class;
        Task UpdateClinicAsync(Clinic clinic);
        Task DeleteClinicAsync(int id);
        Task<bool> ClinicExistsAsync(int id);

        /// <summary>
        /// Check if a user belongs to a specific clinic
        /// </summary>
        Task<bool> UserBelongsToClinicAsync(int userId, int clinicId);
        Task<IEnumerable<Clinic>> GetAllClinicsAsync();
        Task<IEnumerable<Clinic>> GetAllClinicsAsync(int userId);

        /// <summary>
        /// Clinics a patient is eligible to discover: clinics of the patient's
        /// primary plus any active (non-deleted) account memberships (T01, D3).
        /// Clinics without an AccountId are never eligible.
        /// </summary>
        Task<IEnumerable<Clinic>> GetPatientClinicsAsync(int patientId);
    }
}
