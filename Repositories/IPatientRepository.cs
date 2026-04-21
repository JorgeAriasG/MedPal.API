using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.DTOs;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IPatientRepository
    {
        Task<IEnumerable<Patient>> GetAllPatientsAsync(int clinicId, string? search = null, string? sortBy = "name", bool descending = false);
        Task<Patient> GetPatientByIdAsync(int id);
        Task<Patient> AddPatientAsync(Patient patient); // Change return type to PatientDTO
        Task UpdatePatientAsync(int id, Patient patient);
        Task DeletePatientAsync(int id);

        Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

        /// <summary>Returns the AllergyName list for a patient (via PatientDetails). Used for prescription safety validation.</summary>
        Task<IEnumerable<string>> GetPatientAllergyNamesAsync(int patientId, CancellationToken cancellationToken);
    }
}