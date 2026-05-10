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
        private readonly IMapper _mapper;
        private readonly IValidator<AppointmentWriteDTO> _validator;

        public AppointmentService(
            IAppointmentRepository appointmentRepository,
            IMapper mapper,
            IValidator<AppointmentWriteDTO> validator)
        {
            _appointmentRepository = appointmentRepository;
            _mapper = mapper;
            _validator = validator;
        }

        public async Task<IEnumerable<AppointmentReadDTO>> GetAllAppointmentsByIdAsync(int clinicId)
        {
            var appointments = await _appointmentRepository.GetAllAppointmentsByIdAsync(clinicId);
            return _mapper.Map<IEnumerable<AppointmentReadDTO>>(appointments);
        }

        public async Task<AppointmentReadDTO> GetAppointmentByIdAsync(int id)
        {
            var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
            if (appointment == null) return null;

            return _mapper.Map<AppointmentReadDTO>(appointment);
        }

        public async Task<AppointmentReadDTO> CreateAppointmentAsync(AppointmentWriteDTO request)
        {
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
    }
}
