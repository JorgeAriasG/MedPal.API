using AutoMapper;
using MedPal.API.Authorization;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientDetailsController : ControllerBase
    {
        private readonly IPatientDetailsRepository _patientDetailsRepository;
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;

        public PatientDetailsController(IPatientDetailsRepository patientDetailsRepository, IMapper mapper, IAuthorizationService authorizationService)
        {
            _patientDetailsRepository = patientDetailsRepository;
            _mapper = mapper;
            _authorizationService = authorizationService;
        }

        private async Task<bool> CanAccessPatientAsync(int patientId)
        {
            var result = await _authorizationService.AuthorizeAsync(
                User, null, new[] { new PatientAccessRequirement(patientId) });
            return result.Succeeded;
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
            if (patientDetails == null || !await CanAccessPatientAsync(patientDetails.PatientId))
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
            if (!await CanAccessPatientAsync(patientId))
            {
                return NotFound($"Patient Details not found for Patient ID {patientId}");
            }

            var patientDetails = await _patientDetailsRepository.GetPatientDetailsByPatientIdAsync(patientId);

            if (patientDetails == null)
            {
                return NotFound($"Patient Details not found for Patient ID {patientId}");
            }

            var patientDetailsReadDTO = _mapper.Map<PatientDetailsReadDTO>(patientDetails);
            return Ok(patientDetailsReadDTO);
        }

        // GET: api/patientdetails/patient/{patientId}/summary
        [HttpGet("patient/{patientId}/summary")]
        public async Task<ActionResult<PatientDetailsSummaryReadDTO>> GetPatientDetailsSummaryByPatientId(int patientId)
        {
            if (!await CanAccessPatientAsync(patientId))
            {
                return NotFound($"Patient Details not found for Patient ID {patientId}");
            }

            var summary = await _patientDetailsRepository.GetPatientSummaryByPatientIdAsync(patientId);

            if (summary == null)
            {
                return NotFound($"Patient Details not found for Patient ID {patientId}");
            }

            return Ok(summary);
        }

        // POST: api/patientdetails
        [HttpPost]
        public async Task<ActionResult<PatientDetailsReadDTO>> CreatePatientDetails(PatientDetailsWriteDTO patientDetailsWriteDto)
        {
            if (patientDetailsWriteDto.PatientId is not int patientId || !await CanAccessPatientAsync(patientId))
            {
                return Forbid();
            }

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
            if (patientDetails == null || !await CanAccessPatientAsync(patientDetails.PatientId))
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
            if (patientDetails == null || !await CanAccessPatientAsync(patientDetails.PatientId))
            {
                return NotFound();
            }

            _patientDetailsRepository.RemovePatientDetails(patientDetails);
            await _patientDetailsRepository.CompleteAsync();

            return NoContent();
        }
    }
}
