using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class EmergencyContactRepository : TenantAwareRepository<EmergencyContact>, IEmergencyContactRepository
    {
        public EmergencyContactRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<EmergencyContact>> GetAllEmergencyContactsAsync()
        {
            return await ApplyTenantFilter(_context.EmergencyContacts
                .Include(ec => ec.Patient)
                .Where(ec => !ec.IsDeleted))
                .ToListAsync();
        }

        public async Task<EmergencyContact> GetEmergencyContactByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.EmergencyContacts
                .Include(ec => ec.Patient))
                .FirstOrDefaultAsync(ec => ec.Id == id && !ec.IsDeleted);
        }

        public async Task<IEnumerable<EmergencyContact>> GetEmergencyContactsByPatientIdAsync(int patientId)
        {
            return await ApplyTenantFilter(_context.EmergencyContacts
                .Include(ec => ec.Patient)
                .Where(ec => ec.PatientId == patientId && !ec.IsDeleted))
                .OrderByDescending(ec => ec.Priority)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmergencyContact>> GetActiveEmergencyContactsByPatientIdAsync(int patientId)
        {
            return await ApplyTenantFilter(_context.EmergencyContacts
                .Include(ec => ec.Patient)
                .Where(ec => ec.PatientId == patientId && ec.IsActive && !ec.IsDeleted))
                .OrderByDescending(ec => ec.Priority)
                .ToListAsync();
        }

        public async Task<EmergencyContact> AddEmergencyContactAsync(EmergencyContact emergencyContact)
        {
            await _context.EmergencyContacts.AddAsync(emergencyContact);
            return emergencyContact;
        }

        public void UpdateEmergencyContact(EmergencyContact emergencyContact)
        {
            _context.EmergencyContacts.Update(emergencyContact);
        }

        public void RemoveEmergencyContact(EmergencyContact emergencyContact)
        {
            _context.EmergencyContacts.Remove(emergencyContact);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
