using System;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Utils;
using Microsoft.Extensions.Logging;

namespace MedPal.API.Services.Implementations
{
    /// <summary>
    /// Completado de registro del paciente ghost. El consumo del token (pending → used)
    /// es atómico y condicional (UPDATE ... WHERE Status='pending'); junto con el alta del
    /// PatientAuth y la actualización del paciente vive en la misma transacción, por lo que
    /// si el alta falla el token NO se quema.
    /// </summary>
    public class PatientRegistrationService : IPatientRegistrationService
    {
        private readonly IPatientAuthRepository _authRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IPatientRegistrationTokenRepository _tokenRepository;
        private readonly IPatientTokenService _patientTokenService;
        private readonly IRegistrationNotificationService _registrationNotification;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PatientRegistrationService> _logger;

        public PatientRegistrationService(
            IPatientAuthRepository authRepository,
            IPatientRepository patientRepository,
            IPatientRegistrationTokenRepository tokenRepository,
            IPatientTokenService patientTokenService,
            IRegistrationNotificationService registrationNotification,
            IUnitOfWork unitOfWork,
            ILogger<PatientRegistrationService> logger)
        {
            _authRepository = authRepository;
            _patientRepository = patientRepository;
            _tokenRepository = tokenRepository;
            _patientTokenService = patientTokenService;
            _registrationNotification = registrationNotification;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<PatientLoginResponseDTO> CompletePatientRegistrationAsync(CompletePatientRegistrationDTO dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Token))
                throw new ValidationException("Token inválido o ya utilizado.");

            var tokenHash = TokenGenerator.Sha256Hex(dto.Token);
            var token = await _tokenRepository.GetByHashAsync(tokenHash);

            if (token == null || token.Status != "pending")
                throw new ValidationException("Token inválido o ya utilizado.");

            if (token.ExpiresAt < DateTime.UtcNow)
            {
                await _tokenRepository.RevokeAsync(tokenHash);
                throw new ValidationException("Token expirado.");
            }

            var email = dto.Email.Trim().ToLower();
            if (await _authRepository.EmailExistsAsync(email))
                throw new ValidationException("El email ya está registrado.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                var consumed = await _tokenRepository.ConsumeAsync(tokenHash);
                if (consumed == 0)
                    throw new ValidationException("Token inválido o ya utilizado.");

                var auth = new PatientAuth
                {
                    PatientId = token.PatientId,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                    CreatedAt = DateTime.UtcNow
                };

                await _authRepository.CreateAsync(auth);

                var patient = await _patientRepository.GetPatientByIdAsync(token.PatientId);
                if (patient != null)
                {
                    patient.Email = email;
                    if (!string.IsNullOrWhiteSpace(dto.Name))
                        patient.Name = dto.Name;
                    if (!string.IsNullOrWhiteSpace(dto.Lastname))
                        patient.Lastname = dto.Lastname;
                    patient.UpdatedAt = DateTime.UtcNow;

                    await _patientRepository.UpdatePatientAsync(patient.Id, patient);
                }

                await _unitOfWork.CompleteAsync();

                var jwt = _patientTokenService.GeneratePatientToken(patient!, email);

                return new PatientLoginResponseDTO
                {
                    Id = patient!.Id,
                    Name = patient.Name,
                    Lastname = patient.Lastname,
                    Email = email,
                    Token = jwt,
                    Phone = patient.Phone
                };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<string> ResendRegistrationAsync(ResendRegistrationDTO dto)
        {
            var normalized = PhoneNormalizer.Normalize(dto.Phone) ?? dto.Phone;
            var patient = await _patientRepository.FindPatientByPhoneAsync(normalized);
            if (patient == null)
                throw new KeyNotFoundException("No se encontró un paciente con ese teléfono.");

            var pending = (await _tokenRepository.GetPendingByPatientIdAsync(patient.Id)).ToList();
            if (pending.Count == 0)
                throw new ValidationException("No hay un registro pendiente para este teléfono.");

            var latest = pending.First();
            if (latest.ResendCount >= 3)
                throw new ValidationException("Límite de reenvíos alcanzado.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _tokenRepository.RevokePendingByPatientAsync(patient.Id);

                var rawToken = TokenGenerator.GenerateRawToken();
                var newToken = new PatientRegistrationToken
                {
                    PatientId = patient.Id,
                    TokenHash = TokenGenerator.Sha256Hex(rawToken),
                    Status = "pending",
                    ResendCount = latest.ResendCount + 1,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(72)
                };

                await _tokenRepository.CreateAsync(newToken);
                await _unitOfWork.CompleteAsync();

                _ = _registrationNotification.SendRegistrationLinkAsync(patient.Id, rawToken);

                return "Se ha enviado un nuevo mensaje de confirmación.";
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}