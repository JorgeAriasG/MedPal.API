using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class PatientDetailsRepository : TenantAwareRepository<PatientDetails>, IPatientDetailsRepository
    {
        public PatientDetailsRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<PatientDetails>> GetAllPatientDetailsAsync()
        {
            return await ApplyTenantFilter(_context.PatientDetails
                .Include(pd => pd.Patient)
                .Include(pd => pd.MedicalHistories)
                    .ThenInclude(mh => mh.HealthcareProfessional)
                .Include(pd => pd.Allergies))
                .ToListAsync();
        }

        public async Task<PatientDetails> GetPatientDetailsByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.PatientDetails
                .Include(pd => pd.Patient)
                .Include(pd => pd.MedicalHistories)
                    .ThenInclude(mh => mh.HealthcareProfessional)
                .Include(pd => pd.Allergies))
                .FirstOrDefaultAsync(pd => pd.Id == id);
        }

        public async Task<PatientDetails> GetPatientDetailsByPatientIdAsync(int patientId)
        {
            return await ApplyTenantFilter(_context.PatientDetails
                .Include(pd => pd.Patient)
                .Include(pd => pd.MedicalHistories)
                    .ThenInclude(mh => mh.HealthcareProfessional)
                .Include(pd => pd.Allergies))
                .FirstOrDefaultAsync(pd => pd.PatientId == patientId);
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
