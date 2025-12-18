using Microsoft.AspNetCore.Mvc;
using MedPal.API.DTOs;
using MedPal.API.Repositories;
using AutoMapper;
using MedPal.API.Models;
using MedPal.API.Controllers;
using Microsoft.AspNetCore.Authorization;

[ApiController]
[Route("api/[controller]")]
public class ClinicController : BaseController
{
    private readonly IClinicRepository _clinicRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;

    public ClinicController(IClinicRepository clinicRepository, IMapper mapper, IUserService userService)
    {
        _clinicRepository = clinicRepository;
        _mapper = mapper;
        _userService = userService;
    }

    // GET: api/clinic
    [HttpGet]
    [Authorize(Policy = "Clinics.View")]
    public async Task<ActionResult<IEnumerable<ClinicReadDTO>>> GetAllClinicsById()
    {
        var id = int.TryParse(_userService.UserId, out var userId) ? userId : 0;
        var clinics = await _clinicRepository.GetAllClinicsAsync(id);
        var clinicReadDTOs = _mapper.Map<IEnumerable<ClinicReadDTO>>(clinics);
        return Ok(clinicReadDTOs);
    }

    // GET: api/clinic/{id}
    [HttpGet("{id}")]
    [Authorize(Policy = "Clinics.View")]
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
    [Authorize(Policy = "Clinics.Manage")]
    public async Task<ActionResult<ClinicReadDTO>> CreateClinic(int userId, ClinicWriteDTO clinicWriteDto)
    {
        var clinic = _mapper.Map<Clinic>(clinicWriteDto);
        await _clinicRepository.AddClinicAsync(userId, clinic);

        var clinicReadDTO = _mapper.Map<ClinicReadDTO>(clinic);
        return CreatedAtAction(nameof(GetClinicById), new { id = clinicReadDTO.Id }, clinicReadDTO);
    }

    // PUT: api/clinic/{id}
    [HttpPut]
    [Authorize(Policy = "Clinics.Manage")]
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
    [Authorize(Policy = "Clinics.Manage")]
    public async Task<IActionResult> DeleteClinic(int id)
    {
        await _clinicRepository.DeleteClinicAsync(id);

        return NoContent();
    }
}