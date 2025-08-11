using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.DTOs;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patient>> GetAllPatientsAsync(int clinicId);
        Task<Patient> GetPatientByIdAsync(int id);
        Task<Patient> AddPatientAsync(Patient patient); // Change return type to PatientDTO
        Task UpdatePatientAsync(int id, Patient patient);
        Task DeletePatientAsync(int id);
    }
}