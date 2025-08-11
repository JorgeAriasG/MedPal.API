using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.DTOs;
using MedPal.API.Repositories;
using AutoMapper;
using MedPal.API.Models;

[ApiController]
[Route("api/[controller]")]
public class ClinicController : ControllerBase
{
    private readonly IClinicRepository _clinicRepository;
    private readonly IMapper _mapper;

    public ClinicController(IClinicRepository clinicRepository, IMapper mapper)
    {
        _clinicRepository = clinicRepository;
        _mapper = mapper;
    }

    // GET: api/clinic
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClinicReadDTO>>> GetAllClinicsById(int id)
    {
        var clinics = await _clinicRepository.GetAllClinicsAsync(id);
        var clinicReadDTOs = _mapper.Map<IEnumerable<ClinicReadDTO>>(clinics);
        return Ok(clinicReadDTOs);
    }

    // GET: api/clinic/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<ClinicReadDTO>> GetClinicById(int id)
    {
        var clinic = await _clinicRepository.GetClinicByIdAsync(id);
        if (clinic == null)
        {
            return NotFound();
        }
        var clinicReadDTO = _mapper.Map<ClinicReadDTO>(clinic);
        return Ok(clinicReadDTO);
    }

    // POST: api/clinic
    [HttpPost]
    public async Task<ActionResult<ClinicReadDTO>> CreateClinic(int userId, ClinicWriteDTO clinicWriteDto)
    {
        var clinic = _mapper.Map<Clinic>(clinicWriteDto);
        await _clinicRepository.AddClinicAsync(userId, clinic);

        var clinicReadDTO = _mapper.Map<ClinicReadDTO>(clinic);
        return CreatedAtAction(nameof(GetClinicById), new { id = clinicReadDTO.Id }, clinicReadDTO);
    }

    // PUT: api/clinic/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateClinic(ClinicReadDTO clinicReadDTO)
    {
        var clinic = await _clinicRepository.GetClinicByIdAsync(clinicReadDTO.Id);
        if (clinic == null)
        {
            return NotFound();
        }

        var clinicModel = _mapper.Map(clinicReadDTO, clinic);
        await _clinicRepository.UpdateClinicAsync(clinicModel);

        return NoContent();
    }

    // DELETE: api/clinic/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClinic(int id)
    {
        await _clinicRepository.DeleteClinicAsync(id);

        return NoContent();
    }
}