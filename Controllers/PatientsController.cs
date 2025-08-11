using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly IPatientRepository _patientRepository;
        private readonly IMapper _mapper;

        public PatientController(IPatientRepository patientRepository, IMapper mapper)
        {
            _patientRepository = patientRepository;
            _mapper = mapper;
        }

        [HttpGet]
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
            var patientReadDTO = _mapper.Map<PatientReadDTO>(patient);
            return Ok(patientReadDTO);
        }

        [HttpPost]
        public async Task<ActionResult> AddPatient(PatientWriteDTO patientWriteDto)
        {
            var patient = _mapper.Map<Patient>(patientWriteDto);
            patient.Dob.ToLocalTime();
            var createdPatient = await _patientRepository.AddPatientAsync(patient);
            var patientReadDTO = _mapper.Map<PatientReadDTO>(createdPatient);
            return CreatedAtAction(nameof(GetPatientById), new { id = patientReadDTO.Id }, patientReadDTO);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdatePatient(int id, PatientWriteDTO patientWriteDto)
        {
            var patient = _mapper.Map<Patient>(patientWriteDto);
            await _patientRepository.UpdatePatientAsync(id, patient);
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeletePatient(int id)
        {
            await _patientRepository.DeletePatientAsync(id);
            return NoContent();
        }
    }
}