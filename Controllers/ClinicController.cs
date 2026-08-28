using Microsoft.AspNetCore.Mvc;
using MedPal.API.DTOs;
using MedPal.API.Repositories;
using MedPal.API.Services;
using AutoMapper;
using MedPal.API.Models;
using MedPal.API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

[ApiController]
[Route("api/[controller]")]
public class ClinicController : BaseController
{
    private readonly IClinicRepository _clinicRepository;
    private readonly IMapper _mapper;
    private readonly IUserService _userService;
    private readonly ISubscriptionService _subscriptionService;
    private readonly IConfiguration _configuration;

    public ClinicController(IClinicRepository clinicRepository, IMapper mapper, IUserService userService, ISubscriptionService subscriptionService, IConfiguration configuration)
    {
        _clinicRepository = clinicRepository;
        _mapper = mapper;
        _userService = userService;
        _subscriptionService = subscriptionService;
        _configuration = configuration;
    }

    // GET: api/clinic
    [HttpGet]
    [Authorize(Policy = "Clinics.View")]
    public async Task<ActionResult<IEnumerable<ClinicReadDTO>>> GetAllClinicsById()
    {
        var accountId = int.TryParse(_userService.AccountId, out var accId) ? accId : 0;
        if (accountId == 0)
            return Unauthorized("Usuario no tiene AccountId asignado");

        var clinics = await _clinicRepository.GetAllClinicsAsync(accountId);
        var clinicReadDTOs = _mapper.Map<IEnumerable<ClinicReadDTO>>(clinics);
        return Ok(clinicReadDTOs);
    }

    // GET: api/clinic/all - legacy public clinic directory (T01)
    // Retained behind a feature flag for the compatibility window only.
    // When Discovery:AllowAnonymousPublicClinics is false (default, secure-off),
    // the endpoint returns 404 and never enumerates clinics.
    [HttpGet("all")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ClinicReadDTO>>> GetAllClinics()
    {
        if (!_configuration.GetValue<bool>("Discovery:AllowAnonymousPublicClinics"))
            return NotFound();

        var clinics = await _clinicRepository.GetAllClinicsAsync();
        var clinicReadDTOs = _mapper.Map<IEnumerable<ClinicReadDTO>>(clinics);
        return Ok(clinicReadDTOs);
    }

    // GET: api/patient/clinics - tenant-safe patient clinic discovery (T01)
    // Returns only clinics of the patient's primary + active account memberships.
    // No patient_id claim -> 401; no eligible memberships -> empty list, never global.
    [HttpGet("~/api/patient/clinics")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ClinicReadDTO>>> GetPatientClinics()
    {
        var patientIdClaim = User.FindFirst("patient_id");
        if (patientIdClaim == null || !int.TryParse(patientIdClaim.Value, out var patientId))
            return Unauthorized();

        var clinics = await _clinicRepository.GetPatientClinicsAsync(patientId);
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
    [Authorize(Policy = "AdministerClinicPolicy")] // Fase 2: Multi-tenancy policy
    [Authorize(Policy = "Clinics.Manage")]
    public async Task<ActionResult<ClinicReadDTO>> CreateClinic(int userId, ClinicWriteDTO clinicWriteDto)
    {
        var accountId = int.TryParse(_userService.AccountId, out var accId) ? accId : 0;
        if (accountId == 0)
        {
            return Unauthorized("Usuario no tiene AccountId asignado");
        }

        if (!await _subscriptionService.CanAddClinicAsync(accountId))
        {
            var message = await _subscriptionService.GetLimitExceededMessageAsync(accountId, "clinic");
            return BadRequest(new { message });
        }

        var clinic = _mapper.Map<Clinic>(clinicWriteDto);
        clinic.AccountId = accountId;
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