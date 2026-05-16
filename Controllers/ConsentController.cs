using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ConsentController : ControllerBase
    {
        private readonly IPatientConsentService _consentService;
        private readonly IMapper _mapper;

        public ConsentController(IPatientConsentService consentService, IMapper mapper)
        {
            _consentService = consentService;
            _mapper = mapper;
        }

        // GET: api/consent/patient/{patientDetailsId}
        [HttpGet("patient/{patientDetailsId}")]
        public async Task<ActionResult<IEnumerable<ConsentReadDTO>>> GetPatientConsents(int patientDetailsId)
        {
            var consents = await _consentService.GetPatientConsentsAsync(patientDetailsId);
            var dtos = _mapper.Map<IEnumerable<ConsentReadDTO>>(consents);
            return Ok(dtos);
        }

        // GET: api/consent/patient/{patientDetailsId}/active
        [HttpGet("patient/{patientDetailsId}/active")]
        public async Task<ActionResult<IEnumerable<ConsentReadDTO>>> GetActiveConsents(int patientDetailsId)
        {
            var consents = await _consentService.GetActiveConsentsAsync(patientDetailsId);
            var dtos = _mapper.Map<IEnumerable<ConsentReadDTO>>(consents);
            return Ok(dtos);
        }

        // POST: api/consent/grant
        [HttpPost("grant")]
        public async Task<ActionResult<ConsentReadDTO>> GrantConsent([FromBody] ConsentGrantDTO dto)
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            var consent = await _consentService.GrantConsentAsync(
                dto.PatientDetailsId,
                dto.RequestingClinicId,
                dto.OwnerClinicId,
                dto.ConsentScope,
                userId,
                dto.ExpiryDate);

            var readDTO = _mapper.Map<ConsentReadDTO>(consent);
            return Ok(readDTO);
        }

        // POST: api/consent/{id}/revoke
        [HttpPost("{id}/revoke")]
        public async Task<IActionResult> RevokeConsent(int id)
        {
            // Find the consent first
            var consents = await _consentService.GetPatientConsentsAsync(0); // We'll need to find by id
            var consent = consents.FirstOrDefault(c => c.Id == id);
            if (consent == null) return NotFound();

            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            var result = await _consentService.RevokeConsentAsync(
                consent.PatientDetailsId,
                consent.RequestingClinicId,
                consent.OwnerClinicId,
                userId);

            if (!result) return NotFound();
            return NoContent();
        }

        // POST: api/consent/check
        [HttpPost("check")]
        public async Task<ActionResult<bool>> CheckConsent([FromBody] ConsentCheckDTO dto)
        {
            var valid = await _consentService.IsConsentValidAsync(
                dto.PatientDetailsId,
                dto.RequestingClinicId,
                dto.OwnerClinicId);
            return Ok(valid);
        }
    }

    public class ConsentGrantDTO
    {
        public int PatientDetailsId { get; set; }
        public int RequestingClinicId { get; set; }
        public int OwnerClinicId { get; set; }
        public string ConsentScope { get; set; } = "AllRecords";
        public DateTime? ExpiryDate { get; set; }
    }

    public class ConsentCheckDTO
    {
        public int PatientDetailsId { get; set; }
        public int RequestingClinicId { get; set; }
        public int OwnerClinicId { get; set; }
    }
}
