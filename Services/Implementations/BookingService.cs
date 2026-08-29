using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using MedPal.API.DTOs;
using MedPal.API.Enums;
using MedPal.API.Exceptions;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MedPal.API.Services.Implementations
{
    /// <summary>
    /// Orquesta el booking público en una sola transacción (IUnitOfWork):
    /// ghost patient + clinic links + membresía + consentimiento + cita + token de registro.
    /// El envío de WhatsApp ocurre después del commit.
    /// </summary>
    public class BookingService : IBookingService
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IPatientRegistrationTokenRepository _tokenRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAppointmentService _appointmentService;
        private readonly IBookingLinkService _bookingLinkService;
        private readonly IClinicRepository _clinicRepository;
        private readonly IUserRepository _userRepository;
        private readonly IRegistrationNotificationService _registrationNotification;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;
        private readonly ILogger<BookingService> _logger;

        public BookingService(
            IPatientRepository patientRepository,
            IPatientRegistrationTokenRepository tokenRepository,
            IAppointmentRepository appointmentRepository,
            IAppointmentService appointmentService,
            IBookingLinkService bookingLinkService,
            IClinicRepository clinicRepository,
            IUserRepository userRepository,
            IRegistrationNotificationService registrationNotification,
            IUnitOfWork unitOfWork,
            IConfiguration config,
            ILogger<BookingService> logger)
        {
            _patientRepository = patientRepository;
            _tokenRepository = tokenRepository;
            _appointmentRepository = appointmentRepository;
            _appointmentService = appointmentService;
            _bookingLinkService = bookingLinkService;
            _clinicRepository = clinicRepository;
            _userRepository = userRepository;
            _registrationNotification = registrationNotification;
            _unitOfWork = unitOfWork;
            _config = config;
            _logger = logger;
        }

        public async Task<BookingResultDTO> CompleteBookingAsync(int? authPatientId, string? shareToken, BookingCompleteDTO dto)
        {
            var shareCtx = !string.IsNullOrWhiteSpace(shareToken) ? _bookingLinkService.Validate(shareToken) : null;
            if (shareCtx == null && authPatientId == null)
                throw new UnauthorizedAccessException("Se requiere un link de reserva válido o una sesión de paciente.");

            var clinicId = shareCtx?.ClinicId ?? dto.ClinicId ?? 0;
            var doctorId = shareCtx?.DoctorId ?? dto.DoctorId ?? 0;

            if (clinicId == 0 || doctorId == 0)
                throw new ValidationException("Clínica y médico son requeridos.");

            var clinic = await _clinicRepository.GetClinicByIdAsync(clinicId);
            if (clinic == null || clinic.IsDeleted)
                throw new KeyNotFoundException("Clínica no encontrada.");

            // Camino público: la resolución de médico ignora el scope de tenant porque
            // el share-token (firmado) ya autentica la (clínica, médico) y se revalida
            // que el médico pertenezca a esa clínica.
            var doctor = await _userRepository.GetByIdIgnoreTenantAsync(doctorId);
            if (doctor == null || doctor.IsDeleted || doctor.ClinicId != clinicId)
                throw new KeyNotFoundException("Médico no válido para esta clínica.");

            await _unitOfWork.BeginTransactionAsync();

            try
            {
                int patientId;
                var pendingRegistration = false;

                if (authPatientId.HasValue)
                {
                    patientId = authPatientId.Value;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(dto.PatientName) || string.IsNullOrWhiteSpace(dto.PatientPhone))
                        throw new ValidationException("Nombre y teléfono son requeridos.");

                    var nameParts = dto.PatientName.Trim().Split(' ', 2);
                    var firstName = nameParts[0];
                    var lastName = nameParts.Length > 1 ? nameParts[1] : "Sin apellido";

                    var ghost = new Patient
                    {
                        Name = firstName,
                        Middlename = "",
                        Lastname = lastName,
                        Dob = DateTime.UtcNow.AddYears(-30),
                        Gender = "No especificado",
                        Address = "Sin configurar",
                        Phone = PhoneNormalizer.Normalize(dto.PatientPhone) ?? dto.PatientPhone,
                        Email = $"pendiente_{Guid.NewGuid():N}@clinicflow.temp",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };

                    var created = await _patientRepository.AddPatientAsync(ghost);
                    await _patientRepository.AddPatientClinicsAsync(created.Id, new List<int> { clinicId });

                    patientId = created.Id;
                    pendingRegistration = true;
                }

                var accountId = await _patientRepository.GetClinicAccountIdAsync(clinicId);
                if (accountId.HasValue)
                {
                    await _patientRepository.EnsureAccountMembershipAsync(patientId, clinicId);

                    if (dto.ConsentMedicalRecords && dto.ConsentWhatsapp)
                        await _patientRepository.GrantConsentAsync(patientId, accountId.Value);
                }

                var writeDto = new AppointmentWriteDTO
                {
                    PatientId = patientId,
                    ClinicId = clinicId,
                    UserId = doctorId,
                    Date = dto.Date,
                    Time = dto.Time,
                    DurationMinutes = dto.DurationMinutes,
                    Status = AppointmentStatus.Scheduled.ToString()
                };

                var appointment = await _appointmentService.CreateAppointmentAsync(writeDto);

                if (pendingRegistration)
                {
                    var rawToken = TokenGenerator.GenerateRawToken();
                    var token = new PatientRegistrationToken
                    {
                        PatientId = patientId,
                        TokenHash = TokenGenerator.Sha256Hex(rawToken),
                        Status = "pending",
                        CreatedAt = DateTime.UtcNow,
                        ExpiresAt = DateTime.UtcNow.AddHours(72)
                    };

                    await _tokenRepository.CreateAsync(token);
                    await _unitOfWork.CompleteAsync();

                    _ = _registrationNotification.SendRegistrationLinkAsync(patientId, rawToken);
                }
                else
                {
                    await _unitOfWork.CompleteAsync();
                }

                return new BookingResultDTO
                {
                    AppointmentId = appointment.Id,
                    PendingRegistration = pendingRegistration,
                    Message = pendingRegistration
                        ? "Cita agendada. Revisa WhatsApp para completar tu registro."
                        : "Cita agendada exitosamente."
                };
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task<IEnumerable<TimeSlotDTO>> GetPublicAvailabilityAsync(
            string? shareToken, int? clinicId, int? doctorId, DateOnly date, int? authPatientId)
        {
            var shareCtx = !string.IsNullOrWhiteSpace(shareToken) ? _bookingLinkService.Validate(shareToken) : null;
            if (shareCtx == null && authPatientId == null)
                throw new UnauthorizedAccessException("Se requiere un link de reserva válido o una sesión de paciente.");

            var c = shareCtx?.ClinicId ?? clinicId ?? 0;
            var d = shareCtx?.DoctorId ?? doctorId ?? 0;

            if (c == 0 || d == 0)
                throw new ValidationException("Clínica y médico son requeridos.");

            var clinic = await _clinicRepository.GetClinicByIdAsync(c);
            if (clinic == null || clinic.IsDeleted)
                throw new KeyNotFoundException("Clínica no encontrada.");

            // Camino público: médico y overlaps se leen ignorando el tenant filter
            // (autorización vía share-token + revalidación de pertenencia a la clínica).
            var doctor = await _userRepository.GetByIdIgnoreTenantAsync(d);
            if (doctor == null || doctor.IsDeleted || doctor.ClinicId != c)
                throw new KeyNotFoundException("Médico no válido para esta clínica.");

            var existingAppointments = await _appointmentRepository.GetPublicOverlapAsync(c, d, date);
            var doctorAppointments = existingAppointments
                .Where(a => !a.IsDeleted)
                .ToList();

            var slots = new List<TimeSlotDTO>();
            const int slotDuration = 30;
            var currentTime = clinic.Open;

            while (currentTime.AddMinutes(slotDuration) <= clinic.Close)
            {
                var hasOverlap = doctorAppointments.Any(a =>
                {
                    var existingStart = a.Time;
                    var existingEnd = a.Time.AddMinutes(a.DurationMinutes);
                    var newEnd = currentTime.AddMinutes(slotDuration);
                    return currentTime < existingEnd && existingStart < newEnd;
                });

                slots.Add(new TimeSlotDTO
                {
                    Date = date,
                    Time = currentTime,
                    IsAvailable = !hasOverlap
                });

                currentTime = currentTime.AddMinutes(slotDuration);
            }

            return slots;
        }

        public async Task<BookingLinkDTO> GenerateStaffLinkAsync(int userId, BookingLinkStaffDTO dto)
        {
            var clinic = await _clinicRepository.GetClinicByIdAsync(dto.ClinicId);
            if (clinic == null || clinic.IsDeleted)
                throw new KeyNotFoundException("Clínica no encontrada.");

            if (!await _patientRepository.UserBelongsToClinicAsync(userId, dto.ClinicId))
                throw new ForbiddenAccessException("El usuario no pertenece a esta clínica.");

            var doctor = await _userRepository.GetUserByIdAsync(dto.DoctorId);
            if (doctor == null || doctor.IsDeleted || doctor.ClinicId != dto.ClinicId)
                throw new KeyNotFoundException("Médico no válido para esta clínica.");

            var token = _bookingLinkService.Issue(dto.ClinicId, dto.DoctorId);
            var baseUrl = _config["Booking:PublicBaseUrl"] ?? "https://portal.clinicflow.com.mx/booking";
            var url = $"{baseUrl}?sr={Uri.EscapeDataString(token)}";

            return new BookingLinkDTO { Url = url };
        }
    }
}