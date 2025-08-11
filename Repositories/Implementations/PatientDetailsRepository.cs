using MedPal.API.Data;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class PatientDetailsRepository : IPatientDetailsRepository
    {
        private readonly AppDbContext _context;

        public PatientDetailsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PatientDetails>> GetAllPatientDetailsAsync()
        {
            return await _context.PatientDetails.ToListAsync();
        }

        public async Task<PatientDetails> GetPatientDetailsByIdAsync(int id)
        {
            return await _context.PatientDetails.FindAsync(id);
        }

        public async Task<PatientDetails> AddPatientDetailsAsync(PatientDetails patientDetails)
        {
            await _context.PatientDetails.AddAsync(patientDetails);
            return patientDetails;
        }

        public void UpdatePatientDetails(PatientDetails patientDetails)
        {
            _context.PatientDetails.Update(patientDetails);
        }

        public void RemovePatientDetails(PatientDetails patientDetails)
        {
            _context.PatientDetails.Remove(patientDetails);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
