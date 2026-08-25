using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Enums;
using MedPal.API.Models;
using MedPal.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using FluentValidation;
using MedPal.API.Services;
using System.Security.Claims;
using Microsoft.Extensions.Logging;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : BaseController
    {
        private readonly IAppointmentService _appointmentService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(IAppointmentService appointmentService, IAuthorizationService authorizationService, ILogger<AppointmentsController> logger)
        {
            _appointmentService = appointmentService;
            _authorizationService = authorizationService;
            _logger = logger;
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

        // GET: api/appointments/patient/{patientId}
        [HttpGet("patient/{patientId}")]
        [Authorize(Policy = "Appointments.ViewOwn")]
        [Authorize(Policy = "ViewAppointmentsPolicy")] // Fase 2: Multi-tenancy policy
        public async Task<ActionResult<IEnumerable<AppointmentReadDTO>>> GetAppointmentsByPatient(int patientId)
        {
            var appointments = await _appointmentService.GetAppointmentsByPatientIdAsync(patientId);
            return Ok(appointments);
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
        [Authorize(Policy = "Appointments.ViewOwn")]
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
        [Authorize(Policy = "ManagePatientsPolicy")]
        public async Task<ActionResult<AppointmentReadDTO>> CreateAppointment(
            AppointmentWriteDTO appointmentWriteDto,
            [FromServices] IAppointmentReminderService reminderService)
        {
            var appointmentReadDTO = await _appointmentService.CreateAppointmentAsync(appointmentWriteDto);
            await SendCreatedMessageAsync(appointmentReadDTO.Id, reminderService);
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

        // POST: api/appointments/{id}/start
        [HttpPost("{id}/start")]
        [Authorize(Policy = "Appointments.Update")]
        public async Task<IActionResult> StartConsultation(int id)
        {
            var result = await _appointmentService.StartConsultationAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // POST: api/appointments/{id}/complete
        [HttpPost("{id}/complete")]
        [Authorize(Policy = "Appointments.Update")]
        public async Task<IActionResult> CompleteConsultation(int id)
        {
            var result = await _appointmentService.CompleteConsultationAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
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

            var result = await _appointmentService.CancelAppointmentAsync(id);
            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // POST: api/appointments/{id}/patient-reschedule
        [HttpPost("{id}/patient-reschedule")]
        public async Task<IActionResult> PatientReschedule(
            int id,
            AppointmentWriteDTO request,
            [FromServices] IAppointmentReminderService reminderService)
        {
            var patientIdClaim = User.FindFirst("patient_id");
            if (patientIdClaim == null || !int.TryParse(patientIdClaim.Value, out int patientId))
                return Unauthorized();

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return NotFound();

            if (appointment.PatientId != patientId)
                return Forbid();

            if (appointment.Status != AppointmentStatus.Scheduled.ToString() &&
                appointment.Status != AppointmentStatus.Confirmed.ToString())
                return BadRequest(new { message = "No se puede reagendar una cita en este estado" });

            var result = await _appointmentService.RescheduleAppointmentAsync(id, request);
            if (result == null)
                return NotFound();

            await SendCreatedMessageAsync(result.Id, reminderService);
            return Ok(result);
        }

        // POST: api/appointments/patient-book
        [HttpPost("patient-book")]
        public async Task<ActionResult<AppointmentReadDTO>> PatientBook(
            AppointmentWriteDTO request,
            [FromServices] IAppointmentReminderService reminderService)
        {
            var patientIdClaim = User.FindFirst("patient_id");
            if (patientIdClaim == null || !int.TryParse(patientIdClaim.Value, out int patientId))
                return Unauthorized();

            request.PatientId = patientId;

            var result = await _appointmentService.CreateAppointmentAsync(request);
            await SendCreatedMessageAsync(result.Id, reminderService);
            return CreatedAtAction(nameof(GetAppointmentById), new { id = result.Id }, result);
        }

        // POST: api/appointments/{id}/noshow
        [HttpPost("{id}/noshow")]
        [Authorize(Policy = "Appointments.Update")]
        public async Task<IActionResult> MarkNoShow(int id)
        {
            var result = await _appointmentService.MarkNoShowAsync(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // PUT: api/appointments/{id}/reschedule
        [HttpPut("{id}/reschedule")]
        [Authorize(Policy = "Appointments.Update")]
        public async Task<IActionResult> RescheduleAppointment(
            int id,
            AppointmentWriteDTO request,
            [FromServices] IAppointmentReminderService reminderService)
        {
            var result = await _appointmentService.RescheduleAppointmentAsync(id, request);
            if (result == null) return NotFound();
            _ = SendCreatedMessageAsync(result.Id, reminderService);
            return Ok(result);
        }

        // POST: api/appointments/{id}/reminder
        [HttpPost("{id}/reminder")]
        [Authorize(Policy = "Appointments.Update")]
        public async Task<IActionResult> SendReminder(int id, [FromServices] IAppointmentReminderService reminderService)
        {
            var sent = await reminderService.SendReminderForAppointmentAsync(id);
            if (!sent)
                return BadRequest(new { message = "No se pudo enviar el recordatorio (cita no encontrada, sin consentimiento, ya enviado o sin teléfono)" });
            return Ok(new { message = "Recordatorio enviado exitosamente" });
        }

        private async Task SendCreatedMessageAsync(int appointmentId, IAppointmentReminderService reminderService)
        {
            try
            {
                await reminderService.SendCreatedMessageForAppointmentAsync(appointmentId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send created message for Appointment {Id}", appointmentId);
            }
        }
    }
}