using MedPal.API.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MedPal.API.Services
{
    public interface IAppointmentService
    {
        Task<IEnumerable<AppointmentReadDTO>> GetAllAppointmentsByIdAsync(int clinicId, int? userId = null, DateOnly? date = null);
        Task<AppointmentReadDTO> GetAppointmentByIdAsync(int id);
        Task<AppointmentReadDTO> CreateAppointmentAsync(AppointmentWriteDTO request);
        Task<AppointmentReadDTO> UpdateAppointmentAsync(int id, AppointmentWriteDTO request);
        Task<bool> DeleteAppointmentAsync(int id);
        Task<IEnumerable<AppointmentReadDTO>> GetAppointmentsByPatientIdAsync(int patientId);
        Task<AppointmentReadDTO> StartConsultationAsync(int id);
        Task<AppointmentReadDTO> CompleteConsultationAsync(int id);
        Task<AppointmentReadDTO> CancelAppointmentAsync(int id);
        Task<AppointmentReadDTO> MarkNoShowAsync(int id);
        Task<AppointmentReadDTO> RescheduleAppointmentAsync(int id, AppointmentWriteDTO request);
    }
}