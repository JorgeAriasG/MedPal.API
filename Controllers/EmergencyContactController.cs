using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmergencyContactController : ControllerBase
    {
        private readonly IEmergencyContactRepository _emergencyContactRepository;
        private readonly IMapper _mapper;

        public EmergencyContactController(IEmergencyContactRepository emergencyContactRepository, IMapper mapper)
        {
            _emergencyContactRepository = emergencyContactRepository;
            _mapper = mapper;
        }

        // GET: api/emergencycontact
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmergencyContactReadDTO>>> GetAllEmergencyContacts()
        {
            var emergencyContacts = await _emergencyContactRepository.GetAllEmergencyContactsAsync();
            var dtos = _mapper.Map<IEnumerable<EmergencyContactReadDTO>>(emergencyContacts);
            return Ok(dtos);
        }

        // GET: api/emergencycontact/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<EmergencyContactReadDTO>> GetEmergencyContactById(int id)
        {
            var emergencyContact = await _emergencyContactRepository.GetEmergencyContactByIdAsync(id);
            if (emergencyContact == null)
            {
                return NotFound();
            }
            var dto = _mapper.Map<EmergencyContactReadDTO>(emergencyContact);
            return Ok(dto);
        }

        // GET: api/emergencycontact/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        public async Task<ActionResult<IEnumerable<EmergencyContactReadDTO>>> GetEmergencyContactsByPatientId(int patientId)
        {
            var emergencyContacts = await _emergencyContactRepository.GetEmergencyContactsByPatientIdAsync(patientId);
            var dtos = _mapper.Map<IEnumerable<EmergencyContactReadDTO>>(emergencyContacts);
            return Ok(dtos);
        }

        // GET: api/emergencycontact/patient/{patientId}/active
        [HttpGet("patient/{patientId}/active")]
        public async Task<ActionResult<IEnumerable<EmergencyContactReadDTO>>> GetActiveEmergencyContactsByPatientId(int patientId)
        {
            var emergencyContacts = await _emergencyContactRepository.GetActiveEmergencyContactsByPatientIdAsync(patientId);
            var dtos = _mapper.Map<IEnumerable<EmergencyContactReadDTO>>(emergencyContacts);
            return Ok(dtos);
        }

        // POST: api/emergencycontact
        [HttpPost]
        public async Task<ActionResult<EmergencyContactReadDTO>> CreateEmergencyContact([FromBody] EmergencyContactWriteDTO writeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var emergencyContact = _mapper.Map<EmergencyContact>(writeDTO);
            emergencyContact.CreatedAt = DateTime.UtcNow;

            var createdEmergencyContact = await _emergencyContactRepository.AddEmergencyContactAsync(emergencyContact);
            await _emergencyContactRepository.CompleteAsync();

            var readDTO = _mapper.Map<EmergencyContactReadDTO>(createdEmergencyContact);
            return CreatedAtAction(nameof(GetEmergencyContactById), new { id = createdEmergencyContact.Id }, readDTO);
        }

        // PUT: api/emergencycontact/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmergencyContact(int id, [FromBody] EmergencyContactWriteDTO writeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var emergencyContact = await _emergencyContactRepository.GetEmergencyContactByIdAsync(id);
            if (emergencyContact == null)
            {
                return NotFound();
            }

            _mapper.Map(writeDTO, emergencyContact);
            emergencyContact.UpdatedAt = DateTime.UtcNow;

            _emergencyContactRepository.UpdateEmergencyContact(emergencyContact);
            await _emergencyContactRepository.CompleteAsync();

            return NoContent();
        }

        // DELETE: api/emergencycontact/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmergencyContact(int id)
        {
            var emergencyContact = await _emergencyContactRepository.GetEmergencyContactByIdAsync(id);
            if (emergencyContact == null)
            {
                return NotFound();
            }

            emergencyContact.IsDeleted = true;
            emergencyContact.DeletedAt = DateTime.UtcNow;

            _emergencyContactRepository.UpdateEmergencyContact(emergencyContact);
            await _emergencyContactRepository.CompleteAsync();

            return NoContent();
        }
    }
}
