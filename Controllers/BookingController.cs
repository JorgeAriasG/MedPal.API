using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MedPal.API.DTOs;
using MedPal.API.Enums;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Services;
using MedPal.API.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/booking")]
    public class BookingController : BaseController
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IPatientRepository _patientRepository;
        private readonly IPatientAuthRepository _authRepository;
        private readonly IPatientRegistrationTokenRepository _tokenRepository;
        private readonly IBookingLinkService _bookingLinkService;
        private readonly IClinicRepository _clinicRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationChannel _channel;
        private readonly IConfiguration _config;
        private readonly ILogger<BookingController> _logger;

        public BookingController(
            IAppointmentService appointmentService,
            IPatientRepository patientRepository,
            IPatientAuthRepository authRepository,
            IPatientRegistrationTokenRepository tokenRepository,
            IBookingLinkService bookingLinkService,
            IClinicRepository clinicRepository,
            IAppointmentRepository appointmentRepository,
            IUserRepository userRepository,
            INotificationChannel channel,
            IConfiguration config,
            ILogger<BookingController> logger)
        {
            _appointmentService = appointmentService;
            _patientRepository = patientRepository;
            _authRepository = authRepository;
            _tokenRepository = tokenRepository;
            _bookingLinkService = bookingLinkService;
            _clinicRepository = clinicRepository;
            _appointmentRepository = appointmentRepository;
            _userRepository = userRepository;
            _channel = channel;
            _config = config;
            _logger = logger;
        }

        [HttpPost("complete")]
        [AllowAnonymous]
        public async Task<ActionResult<BookingResultDTO>> CompleteBooking([FromBody] BookingCompleteDTO dto)
        {
            var shareCtx = !string.IsNullOrWhiteSpace(dto.Sr) ? _bookingLinkService.Validate(dto.Sr) : null;

            var patientIdClaim = User.FindFirst("patient_id");
            int? authPatientId = null;
            if (patientIdClaim != null && int.TryParse(patientIdClaim.Value, out int pid))
                authPatientId = pid;

            if (shareCtx == null && authPatientId == null)
                return BadRequest(new { message = "Se requiere un link de reserva válido o una sesión de paciente." });

            int clinicId = shareCtx?.ClinicId ?? dto.ClinicId ?? 0;
            int doctorId = shareCtx?.DoctorId ?? dto.DoctorId ?? 0;

            if (clinicId == 0 || doctorId == 0)
                return BadRequest(new { message = "Clínica y médico son requeridos." });

            var clinic = await _clinicRepository.GetClinicByIdAsync(clinicId);
            if (clinic == null || clinic.IsDeleted)
                return NotFound(new { message = "Clínica no encontrada." });

            var doctor = await _userRepository.GetUserByIdAsync(doctorId);
            if (doctor == null || doctor.IsDeleted || doctor.ClinicId != clinicId)
                return NotFound(new { message = "Médico no válido para esta clínica." });

            int patientId;
            bool pendingRegistration = false;

            if (authPatientId.HasValue)
            {
                patientId = authPatientId.Value;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(dto.PatientName) || string.IsNullOrWhiteSpace(dto.PatientPhone))
                    return BadRequest(new { message = "Nombre y teléfono son requeridos." });

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
                var rawToken = GenerateRawToken();
                var tokenHash = Sha256Hex(rawToken);

                var token = new PatientRegistrationToken
                {
                    PatientId = patientId,
                    TokenHash = tokenHash,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddHours(72)
                };

                await _tokenRepository.CreateAsync(token);

                _ = SendRegistrationLinkAsync(patientId, rawToken);
            }

            return Ok(new BookingResultDTO
            {
                AppointmentId = appointment.Id,
                PendingRegistration = pendingRegistration,
                Message = pendingRegistration ? "Cita agendada. Revisa WhatsApp para completar tu registro." : "Cita agendada exitosamente."
            });
        }

        [HttpPost("registration/complete")]
        [AllowAnonymous]
        public async Task<ActionResult<PatientLoginResponseDTO>> CompletePatientRegistration([FromBody] CompletePatientRegistrationDTO dto)
        {
            var tokenHash = Sha256Hex(dto.Token);
            var token = await _tokenRepository.GetByHashAsync(tokenHash);

            if (token == null || token.Status != "pending")
                return BadRequest(new { message = "Token inválido o ya utilizado." });

            if (token.ExpiresAt < DateTime.UtcNow)
            {
                token.Status = "revoked";
                await _tokenRepository.UpdateAsync(token);
                return BadRequest(new { message = "Token expirado." });
            }

            var email = dto.Email.Trim().ToLower();
            if (await _authRepository.EmailExistsAsync(email))
                return BadRequest(new { message = "El email ya está registrado." });

            token.Status = "used";
            token.UsedAt = DateTime.UtcNow;
            await _tokenRepository.UpdateAsync(token);

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

            var jwt = GeneratePatientToken(patient!, email);

            return Ok(new PatientLoginResponseDTO
            {
                Id = patient!.Id,
                Name = patient.Name,
                Lastname = patient.Lastname,
                Email = email,
                Token = jwt,
                Phone = patient.Phone
            });
        }

        [HttpPost("registration/resend")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendRegistration([FromBody] ResendRegistrationDTO dto)
        {
            var normalized = PhoneNormalizer.Normalize(dto.Phone) ?? dto.Phone;
            var patient = await _patientRepository.FindPatientByPhoneAsync(normalized);
            if (patient == null)
                return NotFound(new { message = "No se encontró un paciente con ese teléfono." });

            var pending = (await _tokenRepository.GetPendingByPatientIdAsync(patient.Id)).ToList();
            if (pending.Count == 0)
                return BadRequest(new { message = "No hay un registro pendiente para este teléfono." });

            var latest = pending.First();
            if (latest.ResendCount >= 3)
                return BadRequest(new { message = "Límite de reenvíos alcanzado." });

            foreach (var t in pending)
            {
                t.Status = "revoked";
                await _tokenRepository.UpdateAsync(t);
            }

            var rawToken = GenerateRawToken();
            var newToken = new PatientRegistrationToken
            {
                PatientId = patient.Id,
                TokenHash = Sha256Hex(rawToken),
                Status = "pending",
                ResendCount = latest.ResendCount + 1,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddHours(72)
            };

            await _tokenRepository.CreateAsync(newToken);
            _ = SendRegistrationLinkAsync(patient.Id, rawToken);

            return Ok(new { message = "Se ha enviado un nuevo mensaje de confirmación." });
        }

        [HttpGet("availability")]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<TimeSlotDTO>>> GetPublicAvailability(
            [FromQuery] string? sr = null,
            [FromQuery] int? clinicId = null,
            [FromQuery] int? doctorId = null,
            [FromQuery] DateOnly? date = null)
        {
            if (!date.HasValue)
                return BadRequest(new { message = "La fecha es requerida." });

            var shareCtx = !string.IsNullOrWhiteSpace(sr) ? _bookingLinkService.Validate(sr) : null;

            var patientIdClaim = User.FindFirst("patient_id");
            int? authPatientId = null;
            if (patientIdClaim != null && int.TryParse(patientIdClaim.Value, out int pid))
                authPatientId = pid;

            if (shareCtx == null && authPatientId == null)
                return Unauthorized(new { message = "Se requiere un link de reserva válido o una sesión de paciente." });

            int c = shareCtx?.ClinicId ?? clinicId ?? 0;
            int d = shareCtx?.DoctorId ?? doctorId ?? 0;

            if (c == 0 || d == 0)
                return BadRequest(new { message = "Clínica y médico son requeridos." });

            var clinic = await _clinicRepository.GetClinicByIdAsync(c);
            if (clinic == null || clinic.IsDeleted)
                return NotFound(new { message = "Clínica no encontrada." });

            var doctor = await _userRepository.GetUserByIdAsync(d);
            if (doctor == null || doctor.IsDeleted || doctor.ClinicId != c)
                return NotFound(new { message = "Médico no válido para esta clínica." });

            var existingAppointments = await _appointmentRepository.GetAllAppointmentsByIdAsync(c);
            var doctorAppointments = existingAppointments
                .Where(a => a.UserId == d && a.Date == date && !a.IsDeleted)
                .ToList();

            var openTime = clinic.Open;
            var closeTime = clinic.Close;

            var slots = new List<TimeSlotDTO>();
            const int slotDuration = 30;
            var currentTime = openTime;

            while (currentTime.AddMinutes(slotDuration) <= closeTime)
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
                    Date = date.Value,
                    Time = currentTime,
                    IsAvailable = !hasOverlap
                });

                currentTime = currentTime.AddMinutes(slotDuration);
            }

            return Ok(slots);
        }

        [HttpPost("staff/link")]
        [Authorize]
        public async Task<ActionResult<BookingLinkDTO>> GenerateStaffLink([FromBody] BookingLinkStaffDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            var clinic = await _clinicRepository.GetClinicByIdAsync(dto.ClinicId);
            if (clinic == null || clinic.IsDeleted)
                return NotFound(new { message = "Clínica no encontrada." });

            if (!await _patientRepository.UserBelongsToClinicAsync(userId, dto.ClinicId))
                return Forbid();

            var doctor = await _userRepository.GetUserByIdAsync(dto.DoctorId);
            if (doctor == null || doctor.IsDeleted || doctor.ClinicId != dto.ClinicId)
                return NotFound(new { message = "Médico no válido para esta clínica." });

            var token = _bookingLinkService.Issue(dto.ClinicId, dto.DoctorId);
            var baseUrl = _config["Booking:PublicBaseUrl"] ?? "https://portal.clinicflow.com.mx/booking";
            var url = $"{baseUrl}?sr={Uri.EscapeDataString(token)}";

            return Ok(new BookingLinkDTO { Url = url });
        }

        private static string GenerateRawToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes).Replace("+", "-").Replace("/", "_").TrimEnd('=');
        }

        private static string Sha256Hex(string input)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }

        private async Task SendRegistrationLinkAsync(int patientId, string rawToken)
        {
            try
            {
                var patient = await _patientRepository.GetPatientByIdAsync(patientId);
                if (patient == null || string.IsNullOrWhiteSpace(patient.Phone))
                    return;

                var baseUrl = _config["Booking:CompleteRegistrationBaseUrl"] ?? "https://portal.clinicflow.com.mx/complete";
                var link = $"{baseUrl}?token={Uri.EscapeDataString(rawToken)}";

                var body = $"{patient.Name} | {link}";
                var templateName = _config["WhatsApp:RegistrationTemplateName"] ?? "appointment_created";

                var notification = new NotificationMessage
                {
                    Recipient = PhoneNormalizer.Normalize(patient.Phone) ?? patient.Phone,
                    Body = body,
                    Type = NotificationType.WhatsApp,
                    TemplateName = templateName,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _channel.SendAsync(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error enviando link de completado de registro al paciente {PatientId}", patientId);
            }
        }

        private string GeneratePatientToken(Patient patient, string email)
        {
            var jwtKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, patient.Id.ToString()),
                new(ClaimTypes.Email, email),
                new("patient_id", patient.Id.ToString()),
                new("user_type", "patient"),
                new(ClaimTypes.Role, "Patient"),
                new("role", "Patient"),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(double.Parse(_config["Jwt:ExpiryInMinutes"] ?? "60")),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class BookingLinkStaffDTO
    {
        public int ClinicId { get; set; }
        public int DoctorId { get; set; }
    }
}