using AutoMapper;
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
    public class VitalSignController : ControllerBase
    {
        private readonly IVitalSignRepository _vitalSignRepository;
        private readonly IMapper _mapper;

        public VitalSignController(IVitalSignRepository vitalSignRepository, IMapper mapper)
        {
            _vitalSignRepository = vitalSignRepository;
            _mapper = mapper;
        }

        // GET: api/vitalsign
        [HttpGet]
        public async Task<ActionResult<IEnumerable<VitalSignReadDTO>>> GetAllVitalSigns()
        {
            var vitalSigns = await _vitalSignRepository.GetAllVitalSignsAsync();
            var dtos = _mapper.Map<IEnumerable<VitalSignReadDTO>>(vitalSigns);
            return Ok(dtos);
        }

        // GET: api/vitalsign/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<VitalSignReadDTO>> GetVitalSignById(int id)
        {
            var vitalSign = await _vitalSignRepository.GetVitalSignByIdAsync(id);
            if (vitalSign == null)
            {
                return NotFound();
            }
            var dto = _mapper.Map<VitalSignReadDTO>(vitalSign);
            return Ok(dto);
        }

        // GET: api/vitalsign/patientdetails/{patientDetailsId}
        [HttpGet("patientdetails/{patientDetailsId}")]
        public async Task<ActionResult<IEnumerable<VitalSignReadDTO>>> GetVitalSignsByPatientDetailsId(int patientDetailsId)
        {
            var vitalSigns = await _vitalSignRepository.GetVitalSignsByPatientDetailsIdAsync(patientDetailsId);
            var dtos = _mapper.Map<IEnumerable<VitalSignReadDTO>>(vitalSigns);
            return Ok(dtos);
        }

        // POST: api/vitalsign
        [HttpPost]
        public async Task<ActionResult<VitalSignReadDTO>> CreateVitalSign([FromBody] VitalSignWriteDTO writeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vitalSign = _mapper.Map<VitalSign>(writeDTO);

            if (vitalSign.Weight.HasValue && vitalSign.Height.HasValue && vitalSign.Height.Value > 0)
            {
                var heightM = vitalSign.Height.Value / 100m;
                vitalSign.Bmi = Math.Round(vitalSign.Weight.Value / (heightM * heightM), 1);
            }

            vitalSign.CreatedAt = DateTime.UtcNow;
            vitalSign.UpdatedAt = DateTime.UtcNow;

            var created = await _vitalSignRepository.AddVitalSignAsync(vitalSign);
            await _vitalSignRepository.CompleteAsync();

            var readDTO = _mapper.Map<VitalSignReadDTO>(created);
            return CreatedAtAction(nameof(GetVitalSignById), new { id = created.Id }, readDTO);
        }

        // PUT: api/vitalsign/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVitalSign(int id, [FromBody] VitalSignWriteDTO writeDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var vitalSign = await _vitalSignRepository.GetVitalSignByIdAsync(id);
            if (vitalSign == null)
            {
                return NotFound();
            }

            _mapper.Map(writeDTO, vitalSign);

            if (vitalSign.Weight.HasValue && vitalSign.Height.HasValue && vitalSign.Height.Value > 0)
            {
                var heightM = vitalSign.Height.Value / 100m;
                vitalSign.Bmi = Math.Round(vitalSign.Weight.Value / (heightM * heightM), 1);
            }

            vitalSign.UpdatedAt = DateTime.UtcNow;

            _vitalSignRepository.UpdateVitalSign(vitalSign);
            await _vitalSignRepository.CompleteAsync();

            return NoContent();
        }

        // DELETE: api/vitalsign/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVitalSign(int id)
        {
            var vitalSign = await _vitalSignRepository.GetVitalSignByIdAsync(id);
            if (vitalSign == null)
            {
                return NotFound();
            }

            vitalSign.IsDeleted = true;
            vitalSign.DeletedAt = DateTime.UtcNow;

            _vitalSignRepository.UpdateVitalSign(vitalSign);
            await _vitalSignRepository.CompleteAsync();

            return NoContent();
        }
    }
}
