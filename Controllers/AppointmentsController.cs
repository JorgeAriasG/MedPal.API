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

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : BaseController
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // GET: api/appointments
        [HttpGet]
        [Authorize(Policy = "Appointments.ViewAll")]
        [Authorize(Policy = "ViewAppointmentsPolicy")] // Fase 2: Multi-tenancy policy
        public async Task<ActionResult<IEnumerable<AppointmentReadDTO>>> GetAllAppointmentsById(int clinicId)
        {
            var appointmentReadDTOs = await _appointmentService.GetAllAppointmentsByIdAsync(clinicId);
            return Ok(appointmentReadDTOs);
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
        [Authorize(Policy = "Appointments.Create")]
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
    }
}