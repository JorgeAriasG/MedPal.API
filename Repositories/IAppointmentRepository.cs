using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAllAppointmentsByIdAsync(int clinicId, int? userId = null, DateOnly? date = null);
        Task<Appointment> GetAppointmentByIdAsync(int id);
        Task<Appointment> AddAppointmentAsync(Appointment appointment);
        Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId);
        Task<bool> HasOverlapAsync(int doctorId, DateOnly date, TimeOnly time, int durationMinutes, int? excludeAppointmentId = null);
        void UpdateAppointment(Appointment appointment);
        void RemoveAppointment(Appointment appointment);
        Task<int> CompleteAsync();
    }
}