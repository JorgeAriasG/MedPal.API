using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedPal.API.Models;
using MedPal.API.Data;
using MedPal.API.Services;
using MedPal.API.Repositories.Implementations;

namespace MedPal.API.Repositories
{
    public class AppointmentRepository : TenantAwareRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsByIdAsync(int clinicId, int? userId = null, DateOnly? date = null)
        {
            var query = _context.Appointments.Where(a => a.ClinicId == clinicId);

            if (userId.HasValue)
            {
                query = query.Where(a => a.UserId == userId.Value);
            }

            if (date.HasValue)
            {
                query = query.Where(a => a.Date == date.Value);
            }

            return await ApplyTenantFilter(query).ToListAsync();
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.Appointments)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Appointment>> GetByPatientIdAsync(int patientId)
        {
            return await ApplyTenantFilter(_context.Appointments
                .Where(a => a.PatientId == patientId))
                .OrderByDescending(a => a.Date)
                .ThenByDescending(a => a.Time)
                .ToListAsync();
        }

        public async Task<Appointment> AddAppointmentAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            return appointment;
        }

        public async Task<bool> HasOverlapAsync(int doctorId, DateOnly date, TimeOnly time, int durationMinutes, int? excludeAppointmentId = null)
        {
            var newStart = time;
            var newEnd = time.AddMinutes(durationMinutes);

            var appointments = await ApplyTenantFilter(_context.Appointments)
                .Where(a => a.UserId == doctorId && a.Date == date && !a.IsDeleted)
                .Where(a => excludeAppointmentId == null || a.Id != excludeAppointmentId)
                .ToListAsync();

            foreach (var app in appointments)
            {
                var existingStart = app.Time;
                var existingEnd = app.Time.AddMinutes(app.DurationMinutes);

                // Overlap check: (NewStart < ExistingEnd) AND (ExistingStart < NewEnd)
                if (newStart < existingEnd && existingStart < newEnd)
                {
                    return true;
                }
            }

            return false;
        }

        public void UpdateAppointment(Appointment appointment)
        {
            _context.Appointments.Update(appointment);
        }

        public void RemoveAppointment(Appointment appointment)
        {
            _context.Appointments.Remove(appointment);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}