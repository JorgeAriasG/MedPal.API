using MedPal.API.Models;
using MedPal.API.Repositories;

namespace MedPal.API.Services.Implementations
{
    public class PatientService : IPatientService
    {
        private readonly IPatientAuthRepository _patientAuthRepository;
        private readonly IPatientRegistrationTokenRepository _tokenRepository;

        public PatientService(IPatientAuthRepository patientAuthRepository, IPatientRegistrationTokenRepository tokenRepository)
        {
            _patientAuthRepository = patientAuthRepository;
            _tokenRepository = tokenRepository;
        }

        public async Task<PatientAuth> GetPatientAuthByEmailAsync(string email)
        {
            return await _patientAuthRepository.GetByEmailAsync(email);
        }

        public async Task<PatientAuth> GetPatientAuthByIdAsync(int patientId)
        {
            return await _patientAuthRepository.GetByPatientIdAsync(patientId);
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            return await _patientAuthRepository.EmailExistsAsync(email);
        }

        public async Task<PatientAuth> CreatePatientAuthAsync(PatientAuth patientAuth)
        {
            return await _patientAuthRepository.CreateAsync(patientAuth);
        }

        public async Task UpdateLastLoginAsync(int patientAuthId)
        {
            await _patientAuthRepository.UpdateLastLoginAsync(patientAuthId);
        }

        public async Task<PatientAuth> CreatePatientWithTokenAsync(PatientAuth patientAuth, PatientRegistrationToken token)
        {
            await _patientAuthRepository.CreateAsync(patientAuth);
            await _tokenRepository.UpdateAsync(token);
            return patientAuth;
        }
    }
}