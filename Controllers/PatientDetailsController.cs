using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientDetailsController : ControllerBase
    {
        private readonly IPatientDetailsRepository _patientDetailsRepository;
        private readonly IMapper _mapper;

        public PatientDetailsController(IPatientDetailsRepository patientDetailsRepository, IMapper mapper)
        {
            _patientDetailsRepository = patientDetailsRepository;
            _mapper = mapper;
        }

        // GET: api/patientdetails
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PatientDetailsReadDTO>>> GetAllPatientDetails()
        {
            var patientDetails = await _patientDetailsRepository.GetAllPatientDetailsAsync();
            var patientDetailsReadDTOs = _mapper.Map<IEnumerable<PatientDetailsReadDTO>>(patientDetails);
            return Ok(patientDetailsReadDTOs);
        }

        // GET: api/patientdetails/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<PatientDetailsReadDTO>> GetPatientDetailsById(int id)
        {
            var patientDetails = await _patientDetailsRepository.GetPatientDetailsByIdAsync(id);
            if (patientDetails == null)
            {
                return NotFound();
            }
            var patientDetailsReadDTO = _mapper.Map<PatientDetailsReadDTO>(patientDetails);
            return Ok(patientDetailsReadDTO);
        }

        // GET: api/patientdetails/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<PatientDetailsReadDTO>> GetPatientDetailsByPatientId(int patientId)
        {
            var patientDetails = await _patientDetailsRepository.GetPatientDetailsByPatientIdAsync(patientId);

            if (patientDetails == null)
            {
                // If details don't exist yet for this patient, we might want to return 404
                // or create it? standard REST suggests 404 if the resource (details) is not found.
                return NotFound($"Patient Details not found for Patient ID {patientId}");
            }

            var patientDetailsReadDTO = _mapper.Map<PatientDetailsReadDTO>(patientDetails);
            return Ok(patientDetailsReadDTO);
        }

        // POST: api/patientdetails
        [HttpPost]
        public async Task<ActionResult<PatientDetailsReadDTO>> CreatePatientDetails(PatientDetailsWriteDTO patientDetailsWriteDto)
        {
            var patientDetails = _mapper.Map<PatientDetails>(patientDetailsWriteDto);
            await _patientDetailsRepository.AddPatientDetailsAsync(patientDetails);
            await _patientDetailsRepository.CompleteAsync();

            var patientDetailsReadDTO = _mapper.Map<PatientDetailsReadDTO>(patientDetails);
            return CreatedAtAction(nameof(GetPatientDetailsById), new { id = patientDetailsReadDTO.Id }, patientDetailsReadDTO);
        }

        // PUT: api/patientdetails/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePatientDetails(int id, PatientDetailsWriteDTO patientDetailsWriteDTO)
        {
            var patientDetails = await _patientDetailsRepository.GetPatientDetailsByIdAsync(id);
            if (patientDetails == null)
            {
                return NotFound();
            }

            _mapper.Map(patientDetailsWriteDTO, patientDetails);
            _patientDetailsRepository.UpdatePatientDetails(patientDetails);
            await _patientDetailsRepository.CompleteAsync();

            return NoContent();
        }

        // DELETE: api/patientdetails/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePatientDetails(int id)
        {
            var patientDetails = await _patientDetailsRepository.GetPatientDetailsByIdAsync(id);
            if (patientDetails == null)
            {
                return NotFound();
            }

            _patientDetailsRepository.RemovePatientDetails(patientDetails);
            await _patientDetailsRepository.CompleteAsync();

            return NoContent();
        }
    }
}
