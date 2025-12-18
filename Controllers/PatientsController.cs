using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
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
        [Authorize(Policy = "Patients.ViewAll")]
        public async Task<ActionResult<IEnumerable<PatientReadDTO>>> GetAllPatients(int clinicId)
        {
            var patients = await _patientRepository.GetAllPatientsAsync(clinicId);
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

            // Authorization Check
            var viewAll = await _authorizationService.AuthorizeAsync(User, "Patients.ViewAll");
            if (!viewAll.Succeeded)
            {
                // Check if it's the patient viewing their own record
                var viewOwn = await _authorizationService.AuthorizeAsync(User, "Patients.ViewOwn");
                if (!viewOwn.Succeeded) return Forbid();

                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier); // Or "sub"
                if (userIdClaim == null || patient.UserId != int.Parse(userIdClaim.Value))
                {
                    return Forbid();
                }
            }

            var patientReadDTO = _mapper.Map<PatientReadDTO>(patient);
            return Ok(patientReadDTO);
        }

        [HttpPost]
        [Authorize(Policy = "Patients.Create")]
        public async Task<ActionResult> AddPatient(PatientWriteDTO patientWriteDto)
        {
            var patient = _mapper.Map<Patient>(patientWriteDto);
            patient.Dob.ToLocalTime();
            var createdPatient = await _patientRepository.AddPatientAsync(patient);
            var patientReadDTO = _mapper.Map<PatientReadDTO>(createdPatient);
            return CreatedAtAction(nameof(GetPatientById), new { id = patientReadDTO.Id }, patientReadDTO);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "Patients.Update")]
        public async Task<ActionResult> UpdatePatient(int id, PatientWriteDTO patientWriteDto)
        {
            var patient = _mapper.Map<Patient>(patientWriteDto);
            await _patientRepository.UpdatePatientAsync(id, patient);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "Patients.Delete")]
        public async Task<ActionResult> DeletePatient(int id)
        {
            await _patientRepository.DeletePatientAsync(id);
            return NoContent();
        }
    }
}