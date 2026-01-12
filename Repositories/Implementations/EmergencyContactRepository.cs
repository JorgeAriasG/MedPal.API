using MedPal.API.Data;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class EmergencyContactRepository : IEmergencyContactRepository
    {
        private readonly AppDbContext _context;

        public EmergencyContactRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<EmergencyContact>> GetAllEmergencyContactsAsync()
        {
            return await _context.EmergencyContacts
                .Include(ec => ec.Patient)
                .Where(ec => !ec.IsDeleted)
                .ToListAsync();
        }

        public async Task<EmergencyContact> GetEmergencyContactByIdAsync(int id)
        {
            return await _context.EmergencyContacts
                .Include(ec => ec.Patient)
                .FirstOrDefaultAsync(ec => ec.Id == id && !ec.IsDeleted);
        }

        public async Task<IEnumerable<EmergencyContact>> GetEmergencyContactsByPatientIdAsync(int patientId)
        {
            return await _context.EmergencyContacts
                .Include(ec => ec.Patient)
                .Where(ec => ec.PatientId == patientId && !ec.IsDeleted)
                .OrderByDescending(ec => ec.Priority)
                .ToListAsync();
        }

        public async Task<IEnumerable<EmergencyContact>> GetActiveEmergencyContactsByPatientIdAsync(int patientId)
        {
            return await _context.EmergencyContacts
                .Include(ec => ec.Patient)
                .Where(ec => ec.PatientId == patientId && ec.IsActive && !ec.IsDeleted)
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
