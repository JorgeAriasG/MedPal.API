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
    }
}