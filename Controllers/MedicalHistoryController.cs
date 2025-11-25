using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MedicalHistoryController : ControllerBase
    {
        private readonly IMedicalHistoryRepository _medicalHistoryRepository;
        private readonly IMapper _mapper;

        public MedicalHistoryController(IMedicalHistoryRepository medicalHistoryRepository, IMapper mapper)
        {
            _medicalHistoryRepository = medicalHistoryRepository;
            _mapper = mapper;
        }

        // GET: api/medicalhistory
        [HttpGet]
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
            var medicalHistoryReadDTO = _mapper.Map<MedicalHistoryReadDTO>(medicalHistory);
            return Ok(medicalHistoryReadDTO);
        }

        // TODO: Extract UserId and UserRole from token instead of explicity send the Id on all the requests
        // POST: api/medicalhistory
        [HttpPost]
        public async Task<ActionResult<MedicalHistoryReadDTO>> CreateMedicalHistory(MedicalHistoryWriteDTO medicalHistoryWriteDto)
        {
            var medicalHistory = _mapper.Map<MedicalHistory>(medicalHistoryWriteDto);
            await _medicalHistoryRepository.AddMedicalHistoryAsync(medicalHistory);
            await _medicalHistoryRepository.CompleteAsync();

            var medicalHistoryReadDTO = _mapper.Map<MedicalHistoryReadDTO>(medicalHistory);
            return CreatedAtAction(nameof(GetMedicalHistoryById), new { id = medicalHistoryReadDTO.Id }, medicalHistoryReadDTO);
        }

        // PUT: api/medicalhistory/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMedicalHistory(int id, MedicalHistoryWriteDTO medicalHistoryWriteDto)
        {
            // if (id != medicalHistoryWriteDto.Id)
            // {
            //     return BadRequest();
            // }

            var medicalHistory = await _medicalHistoryRepository.GetMedicalHistoryByIdAsync(id);
            if (medicalHistory == null)
            {
                return NotFound();
            }

            _mapper.Map(medicalHistoryWriteDto, medicalHistory);
            _medicalHistoryRepository.UpdateMedicalHistory(medicalHistory);
            await _medicalHistoryRepository.CompleteAsync();

            return NoContent();
        }

        // DELETE: api/medicalhistory/{id}
        [HttpDelete("{id}")]
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