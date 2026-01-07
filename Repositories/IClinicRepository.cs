using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IClinicRepository
    {
        Task<IEnumerable<Clinic>> GetAllClinicsAsync(int id);
        Task<Clinic> GetClinicByIdAsync(int id);
        Task<UserClinic> GetUserClinicByIdAsync(int id);
        Task<Clinic> AddClinicAsync(int userId, Clinic clinic);
        Task<UserClinic> AddUserClinicAsync(UserClinic userClinic);
        void DetachEntity<T>(T entity) where T : class;
        Task UpdateClinicAsync(Clinic clinic);
        Task DeleteClinicAsync(int id);
        Task<bool> ClinicExistsAsync(int id);

        /// <summary>
        /// Check if a user belongs to a specific clinic
        /// </summary>
        /// <param name="userId">User ID to check</param>
        /// <param name="clinicId">Clinic ID to check</param>
        /// <returns>True if user belongs to the clinic, false otherwise</returns>
        Task<bool> UserBelongsToClinicAsync(int userId, int clinicId);
    }
}