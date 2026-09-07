using MedPal.API.DTOs;
using MedPal.API.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedPal.API.Services
{
    public interface IPatientService
    {
        Task<PatientAuth> GetPatientAuthByEmailAsync(string email);
        Task<PatientAuth> GetPatientAuthByIdAsync(int patientId);
        Task<bool> EmailExistsAsync(string email);
        Task<PatientAuth> CreatePatientAuthAsync(PatientAuth patientAuth);
        Task UpdateLastLoginAsync(int patientAuthId);
        Task<PatientAuth> CreatePatientWithTokenAsync(PatientAuth patientAuth, PatientRegistrationToken token);
    }
}