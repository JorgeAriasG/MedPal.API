using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;

namespace MedPal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientAuthController : BaseController
    {
        private readonly IPatientAuthRepository _patientAuthRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IConfiguration _configuration;

        public PatientAuthController(
            IPatientAuthRepository patientAuthRepository,
            IPatientRepository patientRepository,
            IConfiguration configuration)
        {
            _patientAuthRepository = patientAuthRepository;
            _patientRepository = patientRepository;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<ActionResult<PatientLoginResponseDTO>> Register([FromBody] PatientRegisterDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Email = dto.Email.Trim().ToLower();

            if (await _patientAuthRepository.EmailExistsAsync(dto.Email))
                return BadRequest(new { message = "El email ya está registrado" });

            int? primaryAccountId = null;
            if (dto.ClinicIds != null && dto.ClinicIds.Count > 0)
            {
                primaryAccountId = await _patientRepository.GetClinicAccountIdAsync(dto.ClinicIds[0]);
            }

            var patient = new Patient
            {
                Name = dto.Name,
                Middlename = dto.Middlename ?? "",
                Lastname = dto.Lastname,
                Email = dto.Email,
                Phone = dto.Phone ?? "",
                Address = dto.Address ?? "Sin configurar",
                Dob = dto.Dob ?? DateTime.UtcNow.AddYears(-30),
                Gender = dto.Gender ?? "No especificado",
                AccountId = primaryAccountId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdPatient = await _patientRepository.AddPatientAsync(patient);

            if (dto.ClinicIds != null && dto.ClinicIds.Count > 0)
            {
                await _patientRepository.AddPatientClinicsAsync(createdPatient.Id, dto.ClinicIds);

                if (primaryAccountId.HasValue)
                {
                    // Self-registration implies patient verification and profile-sharing consent.
                    await _patientRepository.CreatePatientAccountAsync(
                        createdPatient.Id, primaryAccountId.Value,
                        isPrimary: true, isVerifiedByPatient: true, consentToShareProfile: true);
                }
            }

            var patientAuth = new PatientAuth
            {
                PatientId = createdPatient.Id,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            await _patientAuthRepository.CreateAsync(patientAuth);

            var token = GeneratePatientToken(createdPatient, dto.Email);

            return Ok(new PatientLoginResponseDTO
            {
                Id = createdPatient.Id,
                Name = createdPatient.Name,
                Lastname = createdPatient.Lastname,
                Email = dto.Email,
                Token = token,
                Phone = createdPatient.Phone
            });
        }

        [HttpPost("signup")]
        [AllowAnonymous]
        public async Task<ActionResult<PatientLoginResponseDTO>> Signup([FromBody] PatientRegisterDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Email = dto.Email.Trim().ToLower();

            if (await _patientAuthRepository.EmailExistsAsync(dto.Email))
                return BadRequest(new { message = "El email ya está registrado" });

            var patient = new Patient
            {
                Name = dto.Name,
                Middlename = dto.Middlename ?? "",
                Lastname = dto.Lastname,
                Email = dto.Email,
                Phone = dto.Phone ?? "",
                Address = dto.Address ?? "Sin configurar",
                Dob = dto.Dob ?? DateTime.UtcNow.AddYears(-30),
                Gender = dto.Gender ?? "No especificado",
                AccountId = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdPatient = await _patientRepository.AddPatientAsync(patient);

            var patientAuth = new PatientAuth
            {
                PatientId = createdPatient.Id,
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                CreatedAt = DateTime.UtcNow
            };

            await _patientAuthRepository.CreateAsync(patientAuth);

            var token = GeneratePatientToken(createdPatient, dto.Email);

            return Ok(new PatientLoginResponseDTO
            {
                Id = createdPatient.Id,
                Name = createdPatient.Name,
                Lastname = createdPatient.Lastname,
                Email = dto.Email,
                Token = token,
                Phone = createdPatient.Phone
            });
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<ActionResult<PatientLoginResponseDTO>> Login([FromBody] PatientLoginDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            dto.Email = dto.Email.Trim().ToLower();

            var auth = await _patientAuthRepository.GetByEmailAsync(dto.Email);
            if (auth == null || !BCrypt.Net.BCrypt.Verify(dto.Password, auth.PasswordHash))
                return Unauthorized(new { message = "Email o contraseña incorrectos" });

            await _patientAuthRepository.UpdateLastLoginAsync(auth.Id);

            var patient = auth.Patient;
            var token = GeneratePatientToken(patient, dto.Email);

            return Ok(new PatientLoginResponseDTO
            {
                Id = patient.Id,
                Name = patient.Name,
                Lastname = patient.Lastname,
                Email = dto.Email,
                Token = token,
                Phone = patient.Phone
            });
        }

        private string GeneratePatientToken(Patient patient, string email)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
                throw new InvalidOperationException("JWT key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, patient.Id.ToString()),
                new Claim(ClaimTypes.Email, email),
                new Claim("patient_id", patient.Id.ToString()),
                new Claim("user_type", "patient"),
                new Claim(ClaimTypes.Role, "Patient"),
                new Claim("role", "Patient"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    double.Parse(_configuration["Jwt:ExpiryInMinutes"] ?? "60")),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
