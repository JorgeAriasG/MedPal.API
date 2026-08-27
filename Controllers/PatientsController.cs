using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.Authorization;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace MedPal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : BaseController
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;

        public PatientController(IPatientRepository patientRepository, IMapper mapper, IAuthorizationService authorizationService)
        {
            _patientRepository = patientRepository;
            _mapper = mapper;
            _authorizationService = authorizationService;
        }

        [HttpGet]
        [Authorize(Policy = "ViewPatientsPolicy")] // Fase 2: Multi-tenancy policy
        public async Task<ActionResult<IEnumerable<PatientReadDTO>>> GetAllPatients(
            [FromQuery] int clinicId, 
            [FromQuery] string? search = null, 
            [FromQuery] string? sortBy = "name", 
            [FromQuery] bool descending = false)
        {
            var hasViewAll = await _authorizationService.AuthorizeAsync(User, "Patients.ViewAll");
            var hasViewAssigned = await _authorizationService.AuthorizeAsync(User, "Patients.ViewAssigned");
            if (!hasViewAll.Succeeded && !hasViewAssigned.Succeeded)
                return Forbid();

            // Staff of the account can see every patient that belongs to their clinics/account.
            var patients = await _patientRepository.GetAllPatientsAsync(clinicId, null, search, sortBy, descending);
            var patientReadDTOs = _mapper.Map<IEnumerable<PatientReadDTO>>(patients);
            return Ok(patientReadDTOs);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PatientReadDTO>> GetPatientById(int id)
        {
            var patient = await _patientRepository.GetPatientByIdAsync(id);
            if (patient == null)
            {
                return NotFound();
            }

            // Account-scoped access: staff of the account or the patient themselves.
            var access = await _authorizationService.AuthorizeAsync(User, null, new[] { new PatientAccessRequirement(id) });
            if (!access.Succeeded)
            {
                return NotFound();
            }

            var patientReadDTO = _mapper.Map<PatientReadDTO>(patient);
            return Ok(patientReadDTO);
        }

        [HttpPost]
        [Authorize(Policy = "Patients.Create")]
        public async Task<ActionResult> AddPatient(PatientWriteDTO patientWriteDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            if (!await _patientRepository.UserBelongsToClinicAsync(userId, patientWriteDto.ClinicIds.FirstOrDefault()))
                return Forbid();

            var patient = _mapper.Map<Patient>(patientWriteDto);
            patient.Phone = PhoneNormalizer.Normalize(patientWriteDto.Phone) ?? patientWriteDto.Phone ?? "";
            patient.Dob.ToLocalTime();
            patient.CreatedByUserId = userId;
            patient.CreatedAt = DateTime.UtcNow;
            var createdPatient = await _patientRepository.AddPatientAsync(patient);
            await _patientRepository.AddPatientClinicsAsync(createdPatient.Id, patientWriteDto.ClinicIds);
            var patientReadDTO = _mapper.Map<PatientReadDTO>(createdPatient);
            return CreatedAtAction(nameof(GetPatientById), new { id = patientReadDTO.Id }, patientReadDTO);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "Patients.Update")]
        public async Task<ActionResult> UpdatePatient(int id, PatientWriteDTO patientWriteDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                return Unauthorized();

            if (patientWriteDto.ClinicIds != null && patientWriteDto.ClinicIds.Count > 0
                && !await _patientRepository.UserBelongsToClinicAsync(userId, patientWriteDto.ClinicIds.FirstOrDefault()))
                return Forbid();

            var patient = _mapper.Map<Patient>(patientWriteDto);
            patient.Phone = PhoneNormalizer.Normalize(patientWriteDto.Phone) ?? patientWriteDto.Phone ?? "";
            await _patientRepository.UpdatePatientAsync(id, patient);
            if (patientWriteDto.ClinicIds != null && patientWriteDto.ClinicIds.Count > 0)
            {
                await _patientRepository.SyncPatientClinicsAsync(id, patientWriteDto.ClinicIds);
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Patients.Delete")]
        public async Task<ActionResult> DeletePatient(int id)
        {
            await _patientRepository.DeletePatientAsync(id);
            return NoContent();
        }

        [HttpGet("me")]
        public async Task<ActionResult<PatientReadDTO>> GetMyProfile()
        {
            var patientIdClaim = User.FindFirst("patient_id");
            if (patientIdClaim == null || !int.TryParse(patientIdClaim.Value, out int patientId))
                return Unauthorized();

            var patient = await _patientRepository.GetPatientByIdAsync(patientId);
            var patientReadDTO = _mapper.Map<PatientReadDTO>(patient);
            return Ok(patientReadDTO);
        }

        [HttpGet("check-email")]
        public async Task<ActionResult<bool>> CheckEmail([FromQuery] string email)
        {
            var exists = await _patientRepository.EmailExistsAsync(email, default);
            return Ok(exists);
        }
    }
}