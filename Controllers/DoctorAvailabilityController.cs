using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctorAvailabilityController : BaseController
    {
        private readonly IUserRepository _userRepository;
        private readonly IClinicRepository _clinicRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IMapper _mapper;

        public DoctorAvailabilityController(
            IUserRepository userRepository,
            IClinicRepository clinicRepository,
            IAppointmentRepository appointmentRepository,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _clinicRepository = clinicRepository;
            _appointmentRepository = appointmentRepository;
            _mapper = mapper;
        }

        [HttpGet("by-clinic/{clinicId}")]
        public async Task<ActionResult<IEnumerable<DoctorReadDTO>>> GetDoctorsByClinic(int clinicId)
        {
            var doctors = await _userRepository.GetDoctorsByClinicAsync(clinicId);
            var dtos = doctors.Select(d => new DoctorReadDTO
            {
                Id = d.Id,
                Name = d.Name,
                Specialty = d.Specialty ?? "Medicina General",
                ProfessionalLicenseNumber = d.ProfessionalLicenseNumber ?? "N/A"
            });
            return Ok(dtos);
        }

        [HttpGet("{doctorId}/availability")]
        public async Task<ActionResult<IEnumerable<TimeSlotDTO>>> GetAvailability(
            int doctorId,
            [FromQuery] DateOnly date,
            [FromQuery] int? clinicId = null)
        {
            if (clinicId == null)
                return BadRequest(new { message = "clinicId es requerido" });

            var clinic = await _clinicRepository.GetClinicByIdAsync(clinicId.Value);
            if (clinic == null)
                return NotFound(new { message = "Clínica no encontrada" });

            var existingAppointments = await _appointmentRepository
                .GetAllAppointmentsByIdAsync(clinicId.Value);

            var doctorAppointments = existingAppointments
                .Where(a => a.UserId == doctorId && a.Date == date && !a.IsDeleted)
                .ToList();

            var openTime = clinic.Open;
            var closeTime = clinic.Close;

            var slots = new List<TimeSlotDTO>();
            const int slotDuration = 30;
            var currentTime = openTime;

            while (currentTime.AddMinutes(slotDuration) <= closeTime)
            {
                var hasOverlap = doctorAppointments.Any(a =>
                {
                    var existingStart = a.Time;
                    var existingEnd = a.Time.AddMinutes(a.DurationMinutes);
                    var newEnd = currentTime.AddMinutes(slotDuration);
                    return currentTime < existingEnd && existingStart < newEnd;
                });

                slots.Add(new TimeSlotDTO
                {
                    Date = date,
                    Time = currentTime,
                    IsAvailable = !hasOverlap
                });

                currentTime = currentTime.AddMinutes(slotDuration);
            }

            return Ok(slots);
        }
    }
}
