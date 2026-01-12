using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using Microsoft.AspNetCore.Authorization;

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

        public MedicalHistoryController(
            IMedicalHistoryRepository medicalHistoryRepository,
            IMapper mapper,
            IAuthorizationService authorizationService,
            IUserService userService)
        {
            _medicalHistoryRepository = medicalHistoryRepository;
            _mapper = mapper;
            _authorizationService = authorizationService;
            _userService = userService;
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
        public async Task<ActionResult<MedicalHistoryReadDTO>> GetMedicalHistoryById(int id)
        {
            var medicalHistory = await _medicalHistoryRepository.GetMedicalHistoryByIdAsync(id);
            if (medicalHistory == null)
            {
                return NotFound();
            }

            // NOM-004 Authorization Check
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, medicalHistory, "MedicalRecords.Read");
            if (!authorizationResult.Succeeded)
            {
                return Forbid();
            }

            var medicalHistoryReadDTO = _mapper.Map<MedicalHistoryReadDTO>(medicalHistory);
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
            return CreatedAtAction(nameof(GetMedicalHistoryById), new { id = medicalHistoryReadDTO.Id }, medicalHistoryReadDTO);
        }

        // PUT: api/medicalhistory/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMedicalHistory(int id, MedicalHistoryWriteDTO medicalHistoryWriteDto)
        {
            var medicalHistory = await _medicalHistoryRepository.GetMedicalHistoryByIdAsync(id);
            if (medicalHistory == null)
            {
                return NotFound();
            }

            // NOM-004 Authorization Check
            var authorizationResult = await _authorizationService.AuthorizeAsync(User, medicalHistory, "MedicalRecords.Update");
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
    }
}