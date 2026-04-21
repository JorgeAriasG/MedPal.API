using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IAppointmentRepository
    {
        Task<IEnumerable<Appointment>> GetAllAppointmentsByIdAsync(int clinicId);
        Task<Appointment> GetAppointmentByIdAsync(int id);
        Task<Appointment> AddAppointmentAsync(Appointment appointment);
        Task<bool> HasOverlapAsync(int doctorId, DateOnly date, TimeOnly time, int durationMinutes, int? excludeAppointmentId = null);
        void UpdateAppointment(Appointment appointment);
        void RemoveAppointment(Appointment appointment);
        Task<int> CompleteAsync();
    }
}