using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using MedPal.API.Authorization;
using MedPal.API.Services;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalHistoryController : ControllerBase
    {
        private readonly IMedicalHistoryRepository _medicalHistoryRepository;
        private readonly IMapper _mapper;
        private readonly IAuthorizationService _authorizationService;
        private readonly IUserService _userService;
        private readonly IPatientConsentService _consentService;
        private readonly IMedicalRecordAccessLogService _accessLogService;

        public MedicalHistoryController(
            IMedicalHistoryRepository medicalHistoryRepository,
            IMapper mapper,
            IAuthorizationService authorizationService,
            IUserService userService,
            IPatientConsentService consentService,
            IMedicalRecordAccessLogService accessLogService)
        {
            _medicalHistoryRepository = medicalHistoryRepository;
            _mapper = mapper;
            _authorizationService = authorizationService;
            _userService = userService;
            _consentService = consentService;
            _accessLogService = accessLogService;
        }

        // GET: api/medicalhistory
        [HttpGet]
        [Authorize(Policy = "MedicalRecords.ViewAll")]
        public async Task<ActionResult<IEnumerable<MedicalHistoryReadDTO>>> GetAllMedicalHistories()
        {
            var medicalHistories = await _medicalHistoryRepository.GetAllMedicalHistoriesAsync();
            var medicalHistoryReadDTOs = _mapper.Map<IEnumerable<MedicalHistoryReadDTO>>(medicalHistories);
            return Ok(medicalHistoryReadDTOs);
        }

        // GET: api/medicalhistory/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "MedicalRecords.Read")]
        public async Task<ActionResult<MedicalHistoryReadDTO>> GetMedicalHistoryById(int id)
        {
            var medicalHistory = await _medicalHistoryRepository.GetMedicalHistoryByIdAsync(id);
            if (medicalHistory == null)
            {
                return NotFound();
            }

            // NOM-004 Authorization Check
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, null, new[] { new MedicalRecordAccessRequirement(id) });
            if (!authorizationResult.Succeeded)
            {
                return NotFound();
            }

            var medicalHistoryReadDTO = _mapper.Map<MedicalHistoryReadDTO>(medicalHistory);
            await LogAccessAsync(medicalHistory.PatientDetailsId, medicalHistory.Id, "Treatment");
            return Ok(medicalHistoryReadDTO);
        }

        // POST: api/medicalhistory
        [HttpPost]
        [Authorize(Policy = "MedicalRecords.Create")]
        public async Task<ActionResult<MedicalHistoryReadDTO>> CreateMedicalHistory(MedicalHistoryWriteDTO medicalHistoryWriteDto)
        {
            var medicalHistory = _mapper.Map<MedicalHistory>(medicalHistoryWriteDto);

            // Set audit fields
            var now = DateTime.UtcNow;
            medicalHistory.CreatedAt = now;
            medicalHistory.UpdatedAt = now;

            // Automatically assign HealthcareProfessionalId and audit user if the current user is a doctor/professional
            if (int.TryParse(_userService.UserId, out int userId))
            {
                medicalHistory.CreatedByUserId = userId;
                medicalHistory.UpdatedByUserId = userId;

                // Logic to check if user is doctor could be here or assumed by policy
                // For now, we assume the creator is the professional if not specified
                if (medicalHistory.HealthcareProfessionalId == null || medicalHistory.HealthcareProfessionalId == 0)
                {
                    medicalHistory.HealthcareProfessionalId = userId;
                }
            }

            await _medicalHistoryRepository.AddMedicalHistoryAsync(medicalHistory);
            await _medicalHistoryRepository.CompleteAsync();

            var medicalHistoryReadDTO = _mapper.Map<MedicalHistoryReadDTO>(medicalHistory);
            await LogAccessAsync(medicalHistory.PatientDetailsId, medicalHistory.Id, "Treatment");
            return CreatedAtAction(nameof(GetMedicalHistoryById), new { id = medicalHistoryReadDTO.Id }, medicalHistoryReadDTO);
        }

        // PUT: api/medicalhistory/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "MedicalRecords.Update")]
        public async Task<IActionResult> UpdateMedicalHistory(int id, MedicalHistoryWriteDTO medicalHistoryWriteDto)
        {
            var medicalHistory = await _medicalHistoryRepository.GetMedicalHistoryByIdAsync(id);
            if (medicalHistory == null)
            {
                return NotFound();
            }

            // NOM-004 Authorization Check (write flows: creator/admin only, no patient self access)
            var authorizationResult = await _authorizationService.AuthorizeAsync(
                User, null, new[] { new MedicalRecordAccessRequirement(id, allowSelfAccess: false) });
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            _mapper.Map(medicalHistoryWriteDto, medicalHistory);
            
            // Update audit fields
            medicalHistory.UpdatedAt = DateTime.UtcNow;
            if (int.TryParse(_userService.UserId, out int userId))
            {
                medicalHistory.UpdatedByUserId = userId;
            }
            
            _medicalHistoryRepository.UpdateMedicalHistory(medicalHistory);
            await _medicalHistoryRepository.CompleteAsync();

            await LogAccessAsync(medicalHistory.PatientDetailsId, medicalHistory.Id, "Administration");
            return NoContent();
        }

        // DELETE: api/medicalhistory/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")] // Strict deletion policy
        public async Task<IActionResult> DeleteMedicalHistory(int id)
        {
            var medicalHistory = await _medicalHistoryRepository.GetMedicalHistoryByIdAsync(id);
            if (medicalHistory == null)
            {
                return NotFound();
            }

            _medicalHistoryRepository.RemoveMedicalHistory(medicalHistory);
            await _medicalHistoryRepository.CompleteAsync();

            return NoContent();
        }

        // GET: api/medicalhistory/patient/{patientDetailsId}
        [HttpGet("patient/{patientDetailsId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MedicalHistoryReadDTO>>> GetMedicalHistoriesByPatientId(int patientDetailsId)
        {
            var histories = await _medicalHistoryRepository.GetMedicalHistoriesByPatientIdAsync(patientDetailsId);

            int.TryParse(_userService.UserId, out int currentUserId);
            int? portalPatientId = null;
            if (int.TryParse(User.FindFirst("patient_id")?.Value, out int pid))
            {
                portalPatientId = pid;
            }

            var filteredHistories = new List<MedicalHistory>();

            foreach (var history in histories)
            {
                if (await CanViewRecordAsync(history, currentUserId, portalPatientId))
                {
                    filteredHistories.Add(history);
                }
            }

            var medicalHistoryReadDTOs = _mapper.Map<IEnumerable<MedicalHistoryReadDTO>>(filteredHistories);
            return Ok(medicalHistoryReadDTOs);
        }

        // GET: api/medicalhistory/patient/{patientDetailsId}/recent
        [HttpGet("patient/{patientDetailsId}/recent")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<MedicalHistorySummaryReadDTO>>> GetRecentMedicalHistoriesByPatientDetailsId(int patientDetailsId, int take = 10)
        {
            if (take <= 0 || take > 50) take = 10;

            var histories = await _medicalHistoryRepository.GetRecentHistoriesByPatientDetailsIdAsync(patientDetailsId, take);

            int.TryParse(_userService.UserId, out int currentUserId);

            var filteredHistories = new List<MedicalHistorySummaryReadDTO>();

            foreach (var history in histories)
            {
                // Creator, admin (MedicalRecords.ViewAll) or consent-based access
                if (history.HealthcareProfessionalId == currentUserId ||
                    (await _authorizationService.AuthorizeAsync(User, "MedicalRecords.ViewAll")).Succeeded ||
                    await _consentService.IsConsentForDoctorValidAsync(patientDetailsId, currentUserId))
                {
                    filteredHistories.Add(history);
                }
            }

            return Ok(filteredHistories);
        }

        private async Task<bool> CanViewRecordAsync(MedicalHistory history, int currentUserId, int? portalPatientId)
        {
            // Author of the record
            if (history.HealthcareProfessionalId == currentUserId || history.CreatedByUserId == currentUserId)
                return true;

            // Admin / supervision
            if ((await _authorizationService.AuthorizeAsync(User, "MedicalRecords.ViewAll")).Succeeded)
                return true;

            // The patient viewing their own medical history
            if (portalPatientId.HasValue && history.PatientDetails?.Patient?.Id == portalPatientId.Value)
                return true;

            // Consent-based access (doctor or clinic-level)
            return await _consentService.IsConsentForDoctorValidAsync(history.PatientDetailsId, currentUserId);
        }

        private async Task LogAccessAsync(int patientDetailsId, int? medicalHistoryId, string purpose)
        {
            if (!int.TryParse(_userService.UserId, out int userId)) return;

            int? clinicId = null;
            if (int.TryParse(User.FindFirst("clinic_id")?.Value, out int cid))
                clinicId = cid;

            var hasConsent = medicalHistoryId.HasValue
                ? await _consentService.IsConsentForDoctorValidAsync(patientDetailsId, userId)
                : false;

            var accessLog = new MedicalRecordAccessLog
            {
                UserId = userId,
                MedicalHistoryId = medicalHistoryId,
                PatientDetailsId = patientDetailsId,
                AccessTime = DateTime.UtcNow,
                Purpose = purpose,
                AccessingClinicId = clinicId ?? 0,
                MedicalRecordOwnerClinicId = clinicId ?? 0,
                HadValidConsent = hasConsent,
                IpAddress = HttpContext?.Connection?.RemoteIpAddress?.ToString(),
                SessionId = HttpContext?.Request?.Headers["X-Session-Id"].FirstOrDefault()
            };

            await _accessLogService.LogAccessAsync(accessLog);
        }
    }
}