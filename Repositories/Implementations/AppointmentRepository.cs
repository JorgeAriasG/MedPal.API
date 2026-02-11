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

        public async Task<IEnumerable<Appointment>> GetAllAppointmentsByIdAsync(int clinicId)
        {
            return await ApplyTenantFilter(_context.Appointments.Where(a => a.ClinicId == clinicId)).ToListAsync();
        }

        public async Task<Appointment> GetAppointmentByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.Appointments)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<Appointment> AddAppointmentAsync(Appointment appointment)
        {
            await _context.Appointments.AddAsync(appointment);
            return appointment;
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