using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using MedPal.API.Services;
using System.Security.Claims;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : BaseController
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAuthorizationService _authorizationService;

        public AppointmentsController(IAppointmentService appointmentService, IAuthorizationService authorizationService)
        {
            _appointmentService = appointmentService;
            _authorizationService = authorizationService;
        }

        // GET: api/appointments?clinicId={clinicId}&date={date}
        [HttpGet]
        [Authorize(Policy = "ViewAppointmentsPolicy")] // Fase 2: Multi-tenancy policy
        public async Task<ActionResult<IEnumerable<AppointmentReadDTO>>> GetAllAppointmentsById(
            [FromQuery] int clinicId,
            [FromQuery] DateOnly? date = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            var hasViewAll = await _authorizationService.AuthorizeAsync(User, "Appointments.ViewAll");

            int? userId = null;
            if (!hasViewAll.Succeeded && userIdClaim != null && int.TryParse(userIdClaim.Value, out int uid))
                userId = uid;

            var appointmentReadDTOs = await _appointmentService.GetAllAppointmentsByIdAsync(clinicId, userId, date);
            return Ok(appointmentReadDTOs);
        }

        // GET: api/appointments/my
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<AppointmentReadDTO>>> GetMyAppointments()
        {
            var patientIdClaim = User.FindFirst("patient_id");
            if (patientIdClaim == null || !int.TryParse(patientIdClaim.Value, out int patientId))
                return Unauthorized();

            var appointments = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);
            return Ok(appointments);
        }

        // GET: api/appointments/{id}
        [HttpGet("{id}")]
        [Authorize(Policy = "Appointments.ViewAll")]
        [Authorize(Policy = "ViewAppointmentsPolicy")] // Fase 2: Multi-tenancy policy
        public async Task<ActionResult<AppointmentReadDTO>> GetAppointmentById(int id)
        {
            var appointmentReadDTO = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointmentReadDTO == null)
            {
                return NotFound();
            }
            return Ok(appointmentReadDTO);
        }

        // POST: api/appointments
        [HttpPost]
        [Authorize(Policy = "Appointments.Create")]
        [Authorize(Policy = "ManagePatientsPolicy")] // Fase 2: Multi-tenancy policy
        public async Task<ActionResult<AppointmentReadDTO>> CreateAppointment(AppointmentWriteDTO appointmentWriteDto)
        {
            var appointmentReadDTO = await _appointmentService.CreateAppointmentAsync(appointmentWriteDto);
            return CreatedAtAction(nameof(GetAppointmentById), new { id = appointmentReadDTO.Id }, appointmentReadDTO);
        }

        // PUT: api/appointments/{id}
        [HttpPut("{id}")]
        [Authorize(Policy = "Appointments.Update")]
        public async Task<IActionResult> UpdateAppointment(int id, AppointmentWriteDTO appointmentWriteDto)
        {
            var result = await _appointmentService.UpdateAppointmentAsync(id, appointmentWriteDto);
            if (result == null)
            {
                return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/appointments/{id}
        [HttpDelete("{id}")]
        [Authorize(Policy = "Appointments.Cancel")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var success = await _appointmentService.DeleteAppointmentAsync(id);
            if (!success)
            {
                return NotFound();
            }

            return NoContent();
        }

        // POST: api/appointments/{id}/cancel
        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> CancelMyAppointment(int id)
        {
            var patientIdClaim = User.FindFirst("patient_id");
            if (patientIdClaim == null || !int.TryParse(patientIdClaim.Value, out int patientId))
                return Unauthorized();

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return NotFound();

            var success = await _appointmentService.DeleteAppointmentAsync(id);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}