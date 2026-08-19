using MedPal.API.Data;
using MedPal.API.DTOs;
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

        // PatientDetails has no AccountId column; tenancy must be resolved through Patient.
        private IQueryable<PatientDetails> ApplyAccountFilter(IQueryable<PatientDetails> query)
        {
            if (_tenantContext.IsSuperAdmin || !_tenantContext.CurrentAccountId.HasValue)
                return query;

            var accountId = _tenantContext.CurrentAccountId.Value;
            var clinicId = _tenantContext.CurrentClinicId;

            return query.Where(pd =>
                pd.Patient.AccountId == accountId ||
                (pd.Patient.AccountId == null && clinicId.HasValue &&
                 pd.Patient.PatientClinics.Any(pc => pc.ClinicId == clinicId.Value && !pc.IsDeleted)));
        }

        public async Task<IEnumerable<PatientDetails>> GetAllPatientDetailsAsync()
        {
            return await ApplyAccountFilter(_context.PatientDetails
                .AsNoTracking()
                .AsSplitQuery()
                .Include(pd => pd.Patient)
                .Include(pd => pd.MedicalHistories)
                    .ThenInclude(mh => mh.HealthcareProfessional)
                .Include(pd => pd.Allergies))
                .ToListAsync();
        }

        public async Task<PatientDetails> GetPatientDetailsByIdAsync(int id)
        {
            return await ApplyAccountFilter(_context.PatientDetails
                .AsNoTracking()
                .AsSplitQuery()
                .Include(pd => pd.Patient)
                .Include(pd => pd.MedicalHistories)
                    .ThenInclude(mh => mh.HealthcareProfessional)
                .Include(pd => pd.Allergies))
                .FirstOrDefaultAsync(pd => pd.Id == id);
        }

        public async Task<PatientDetails> GetPatientDetailsByPatientIdAsync(int patientId)
        {
            return await ApplyAccountFilter(_context.PatientDetails
                .AsNoTracking()
                .AsSplitQuery()
                .Include(pd => pd.Patient)
                .Include(pd => pd.MedicalHistories)
                    .ThenInclude(mh => mh.HealthcareProfessional)
                .Include(pd => pd.Allergies))
                .FirstOrDefaultAsync(pd => pd.PatientId == patientId);
        }

        public async Task<PatientDetailsSummaryReadDTO> GetPatientSummaryByPatientIdAsync(int patientId)
        {
            return await ApplyAccountFilter(_context.PatientDetails)
                .AsNoTracking()
                .Where(pd => pd.PatientId == patientId)
                .Select(pd => new PatientDetailsSummaryReadDTO
                {
                    Id = pd.Id,
                    PatientId = pd.PatientId,
                    AntecedentsData = pd.AntecedentsData,
                    Patient = new PatientSummaryReadDTO
                    {
                        Id = pd.Patient.Id,
                        Name = pd.Patient.Name,
                        Middlename = pd.Patient.Middlename,
                        Lastname = pd.Patient.Lastname,
                        Email = pd.Patient.Email,
                        Phone = pd.Patient.Phone,
                        Gender = pd.Patient.Gender,
                        Weight = pd.Patient.Weight,
                        Height = pd.Patient.Height
                    }
                })
                .FirstOrDefaultAsync();
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
