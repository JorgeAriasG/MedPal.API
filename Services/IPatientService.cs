using MedPal.API.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedPal.API.Services
{
    public interface IPatientService
    {
        Task<IEnumerable<PatientReadDTO>> GetAllPatientsAsync(int pageNumber = 1, int pageSize = 10);
        Task<PatientReadDTO> GetPatientByIdAsync(int id);
        Task<PatientReadDTO> CreatePatientAsync(PatientWriteDTO request);
        Task<PatientReadDTO> UpdatePatientAsync(int id, PatientWriteDTO request);
        Task<bool> DeletePatientAsync(int id);
    }
}