using AutoMapper;
using MedPal.API.Authorization;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using MedPal.API.Services;
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
        private readonly IPatientDetailsRepository _patientDetailsRepository;
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;
        private readonly IMedicalRecordAccessLogService _accessLogService;

        public VitalSignController(
            IVitalSignRepository vitalSignRepository,
            IPatientDetailsRepository patientDetailsRepository,
            IMapper mapper,
            IAuthorizationService authorizationService,
            IMedicalRecordAccessLogService accessLogService)
        {
            _vitalSignRepository = vitalSignRepository;
            _patientDetailsRepository = patientDetailsRepository;
            _mapper = mapper;
            _authorizationService = authorizationService;
            _accessLogService = accessLogService;
        }

        private async Task<bool> CanAccessPatientAsync(int patientId)
        {
            var result = await _authorizationService.AuthorizeAsync(
                User, null, new[] { new PatientAccessRequirement(patientId) });
            return result.Succeeded;
        }

        private async Task<bool> CanAccessPatientDetailsAsync(int patientDetailsId)
        {
            var details = await _patientDetailsRepository.GetPatientDetailsByIdAsync(patientDetailsId);
            if (details == null)
                return false;
            return await CanAccessPatientAsync(details.PatientId);
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
            if (vitalSign == null || !await CanAccessPatientDetailsAsync(vitalSign.PatientDetailsId))
            {
                return NotFound();
            }
            var dto = _mapper.Map<VitalSignReadDTO>(vitalSign);
            await LogAccessAsync(vitalSign.PatientDetailsId, "Treatment");
            return Ok(dto);
        }

        // GET: api/vitalsign/patientdetails/{patientDetailsId}
        [HttpGet("patientdetails/{patientDetailsId}")]
        public async Task<ActionResult<IEnumerable<VitalSignReadDTO>>> GetVitalSignsByPatientDetailsId(int patientDetailsId)
        {
            if (!await CanAccessPatientDetailsAsync(patientDetailsId))
            {
                return NotFound();
            }

            var vitalSigns = await _vitalSignRepository.GetVitalSignsByPatientDetailsIdAsync(patientDetailsId);
            var dtos = _mapper.Map<IEnumerable<VitalSignReadDTO>>(vitalSigns);
            await LogAccessAsync(patientDetailsId, "Treatment");
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

            if (!await CanAccessPatientDetailsAsync(writeDTO.PatientDetailsId))
            {
                return Forbid();
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
            if (vitalSign == null || !await CanAccessPatientDetailsAsync(vitalSign.PatientDetailsId))
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
            if (vitalSign == null || !await CanAccessPatientDetailsAsync(vitalSign.PatientDetailsId))
            {
                return NotFound();
            }

            vitalSign.IsDeleted = true;
            vitalSign.DeletedAt = DateTime.UtcNow;

            _vitalSignRepository.UpdateVitalSign(vitalSign);
            await _vitalSignRepository.CompleteAsync();

            return NoContent();
        }

        private async Task LogAccessAsync(int patientDetailsId, string purpose)
        {
            int? userId = null;
            if (int.TryParse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value, out int uid))
                userId = uid;

            int? clinicId = null;
            if (int.TryParse(User.FindFirst("clinic_id")?.Value, out int cid))
                clinicId = cid;

            var accessLog = new MedicalRecordAccessLog
            {
                UserId = userId ?? 0,
                PatientDetailsId = patientDetailsId,
                AccessTime = DateTime.UtcNow,
                Purpose = purpose,
                AccessingClinicId = clinicId ?? 0,
                MedicalRecordOwnerClinicId = clinicId ?? 0,
                HadValidConsent = true,
                IpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                SessionId = HttpContext?.Request?.Headers["X-Session-Id"].FirstOrDefault()
            };

            await _accessLogService.LogAccessAsync(accessLog);
        }
    }
}
