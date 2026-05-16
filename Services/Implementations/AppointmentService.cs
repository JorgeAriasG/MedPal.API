using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using FluentValidation;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Repositories;

namespace MedPal.API.Services.Implementations
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly IMapper _mapper;
        private readonly IValidator<AppointmentWriteDTO> _validator;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IPatientRepository patientRepository,
            IMapper mapper,
            IValidator<AppointmentWriteDTO> validator)
        {
            _appointmentRepository = appointmentRepository;
            _patientRepository = patientRepository;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<IEnumerable<AppointmentReadDTO>> GetAllAppointmentsByIdAsync(int clinicId, int? userId = null, DateOnly? date = null)
        {
            var appointments = await _appointmentRepository.GetAllAppointmentsByIdAsync(clinicId, userId, date);
            return _mapper.Map<IEnumerable<AppointmentReadDTO>>(appointments);
        }

        public async Task<AppointmentReadDTO> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null) return null;

            return _mapper.Map<AppointmentReadDTO>(appointment);
        }

        public async Task<IEnumerable<AppointmentReadDTO>> GetAppointmentsByPatientIdAsync(int patientId)
        {
            var appointments = await _appointmentRepository.GetByPatientIdAsync(patientId);
            return _mapper.Map<IEnumerable<AppointmentReadDTO>>(appointments);
        }

        public async Task<AppointmentReadDTO> CreateAppointmentAsync(AppointmentWriteDTO request)
        {
            // Creación Fantasma: si no viene PatientId pero sí PatientName,
            // crear el paciente automáticamente en background
            if (!request.PatientId.HasValue && !string.IsNullOrWhiteSpace(request.PatientName))
            {
                var ghostPatient = await CreateGhostPatientAsync(request);
                request.PatientId = ghostPatient.Id;
            }

            await _validator.ValidateAndThrowAsync(request);

            var appointment = _mapper.Map<Appointment>(request);
            appointment.CreatedAt = DateTime.UtcNow;
            appointment.UpdatedAt = DateTime.UtcNow;

            await _appointmentRepository.AddAppointmentAsync(appointment);
            await _appointmentRepository.CompleteAsync();

            return _mapper.Map<AppointmentReadDTO>(appointment);
        }

        public async Task<AppointmentReadDTO> UpdateAppointmentAsync(int id, AppointmentWriteDTO request)
        {
            await _validator.ValidateAndThrowAsync(request);

            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null) return null;

            appointment.UpdatedAt = DateTime.UtcNow;
            var originalPatientId = appointment.PatientId;
            var originalUserId = appointment.UserId;

            _mapper.Map(request, appointment);

            if (!request.PatientId.HasValue) appointment.PatientId = originalPatientId;
            if (!request.UserId.HasValue) appointment.UserId = originalUserId;

            _appointmentRepository.UpdateAppointment(appointment);
            await _appointmentRepository.CompleteAsync();

            return _mapper.Map<AppointmentReadDTO>(appointment);
        }

        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null) return false;

            _appointmentRepository.RemoveAppointment(appointment);
            await _appointmentRepository.CompleteAsync();
            
            return true;
        }

        /// <summary>
        /// Crea un paciente "fantasma" con datos mínimos a partir del nombre y teléfono
        /// proporcionados en la cita. El médico puede completar el perfil después.
        /// Sigue la estrategia UX de "Creación Fantasma" — cero fricción.
        /// </summary>
        private async Task<Patient> CreateGhostPatientAsync(AppointmentWriteDTO request)
        {
            // Parsear nombre: "Juan Pérez" → Name="Juan", Lastname="Pérez"
            var nameParts = request.PatientName!.Trim().Split(' ', 2);
            var firstName = nameParts[0];
            var lastName = nameParts.Length > 1 ? nameParts[1] : "Sin apellido";

            var ghostPatient = new Patient
            {
                Name = firstName,
                Middlename = "",
                Lastname = lastName,
                Dob = DateTime.UtcNow.AddYears(-30), // Default: 30 años (el médico actualizará)
                Gender = "No especificado",
                Address = "Sin configurar",
                Phone = request.PatientPhone ?? "",
                Email = $"pendiente_{Guid.NewGuid():N}@clinicflow.temp", // Email temporal único
                AccountId = null, // Se asignará vía tenant context si aplica
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdPatient = await _patientRepository.AddPatientAsync(ghostPatient);

            if (request.ClinicId.HasValue)
            {
                await _patientRepository.AddPatientClinicsAsync(createdPatient.Id, new List<int> { request.ClinicId.Value });
            }

            return createdPatient;
        }
    }
}
