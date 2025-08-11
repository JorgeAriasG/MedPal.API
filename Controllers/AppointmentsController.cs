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
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IMapper _mapper;

        public AppointmentsController(IAppointmentRepository appointmentRepository, IMapper mapper)
        {
            _appointmentRepository = appointmentRepository;
            _mapper = mapper;
        }

        // GET: api/appointments
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AppointmentReadDTO>>> GetAllAppointmentsById(int clinicId)
        {
            var appointments = await _appointmentRepository.GetAllAppointmentsByIdAsync(clinicId);
            var appointmentReadDTOs = _mapper.Map<IEnumerable<AppointmentReadDTO>>(appointments);
            return Ok(appointmentReadDTOs);
        }

        // GET: api/appointments/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<AppointmentReadDTO>> GetAppointmentById(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }
            var appointmentReadDTO = _mapper.Map<AppointmentReadDTO>(appointment);
            return Ok(appointmentReadDTO);
        }

        // POST: api/appointments
        [HttpPost]
        public async Task<ActionResult<AppointmentReadDTO>> CreateAppointment(AppointmentWriteDTO appointmentWriteDto)
        {
            var appointment = _mapper.Map<Appointment>(appointmentWriteDto);

            // Assign parsed date and time after mapping
            appointment.CreatedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _appointmentRepository.AddAppointmentAsync(appointment);
            await _appointmentRepository.CompleteAsync();

            var appointmentReadDTO = _mapper.Map<AppointmentReadDTO>(appointment);
            return CreatedAtAction(nameof(GetAppointmentById), new { id = appointmentReadDTO.Id }, appointmentReadDTO);
        }

        // PUT: api/appointments/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAppointment(int id, AppointmentWriteDTO appointmentWriteDto)
        {
            // if (id != appointmentWriteDto.Id)
            // {
            //     return BadRequest();
            // }

            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            appointment.UpdatedAt = DateTime.UtcNow;
            if (appointment == null)
            {
                return NotFound();
            }

            _mapper.Map(appointmentWriteDto, appointment);
            _appointmentRepository.UpdateAppointment(appointment);
            await _appointmentRepository.CompleteAsync();

            return NoContent();
        }

        // DELETE: api/appointments/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null)
            {
                return NotFound();
            }

            _appointmentRepository.RemoveAppointment(appointment);
            await _appointmentRepository.CompleteAsync();

            return NoContent();
        }
    }
}