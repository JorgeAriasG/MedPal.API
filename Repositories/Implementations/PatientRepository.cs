using AutoMapper;
using MedPal.API.Data;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class PatientRepository : TenantAwareRepository<Patient>, IPatientRepository
    {
        private readonly IMapper _mapper;

        public PatientRepository(AppDbContext context, IMapper mapper, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
            _mapper = mapper;
        }

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync(int clinicId, int? userId = null, string? search = null, string? sortBy = "name", bool descending = false)
        {
            IQueryable<Patient> query = _context.Patients
                .Where(p => p.PatientClinics.Any(pc => pc.ClinicId == clinicId));

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(search) || 
                    p.Lastname.ToLower().Contains(search) || 
                    p.Phone.Contains(search));
            }

            switch (sortBy?.ToLower())
            {
                case "lastname":
                    query = descending ? query.OrderByDescending(p => p.Lastname) : query.OrderBy(p => p.Lastname);
                    break;
                case "createdat":
                    query = descending ? query.OrderByDescending(p => p.Id) : query.OrderBy(p => p.Id);
                    break;
                case "name":
                default:
                    query = descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name);
                    break;
            }

            return await query.ToListAsync();
        }

        public async Task<Patient?> GetPatientByIdAsync(int id)
        {
            // No tenant filter here: access is decided by the PatientAccessHandler.
            return await _context.Patients
                .AsNoTracking()
                .Include(p => p.PatientClinics)
                .Include(p => p.PatientAccounts)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<Patient> AddPatientAsync(Patient patient)
        {
            patient.PatientDetails = new PatientDetails
            {
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Patients.AddAsync(patient);
            await _context.SaveChangesAsync();
            return patient;
        }

        public async Task UpdatePatientAsync(int id, Patient patient)
        {
            patient.Id = id;
            var existingPatient = await _context.Patients.FindAsync(id);
            if (existingPatient != null)
            {
                _mapper.Map(patient, existingPatient);
                _context.Patients.Update(existingPatient);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeletePatientAsync(int id)
        {
            var patient = await _context.Patients.FindAsync(id);
            if (patient != null)
            {
                _context.Patients.Remove(patient);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
        {
            return await _context.Patients
                .AsNoTracking()
                .AnyAsync(p => p.Email.ToLower() == email.ToLower(), cancellationToken);
        }

        public async Task<IEnumerable<string>> GetPatientAllergyNamesAsync(int patientId, CancellationToken cancellationToken)
        {
            var details = await _context.PatientDetails
                .AsNoTracking()
                .Include(pd => pd.Allergies)
                .FirstOrDefaultAsync(pd => pd.PatientId == patientId, cancellationToken);

            if (details == null) return Enumerable.Empty<string>();

            return details.Allergies
                .Where(a => !a.IsDeleted)
                .Select(a => a.AllergyName.ToLower())
                .ToList();
        }

        public async Task AddPatientClinicsAsync(int patientId, List<int> clinicIds)
        {
            var entries = clinicIds.Select(c => new PatientClinic
            {
                PatientId = patientId,
                ClinicId = c,
                CreatedAt = DateTime.UtcNow
            });
            await _context.PatientClinics.AddRangeAsync(entries);
            await SyncPatientAccountsAsync(patientId, clinicIds);
        }

        public async Task SyncPatientClinicsAsync(int patientId, List<int> newClinicIds)
        {
            var existing = await _context.PatientClinics
                .Where(pc => pc.PatientId == patientId && !pc.IsDeleted)
                .ToListAsync();

            var toDelete = existing.Where(pc => !newClinicIds.Contains(pc.ClinicId));
            foreach (var pc in toDelete)
            {
                pc.IsDeleted = true;
                pc.DeletedAt = DateTime.UtcNow;
            }

            var existingIds = existing.Select(pc => pc.ClinicId).ToHashSet();
            var toAdd = newClinicIds.Where(id => !existingIds.Contains(id))
                .Select(c => new PatientClinic
                {
                    PatientId = patientId,
                    ClinicId = c,
                    CreatedAt = DateTime.UtcNow
                });

            await _context.PatientClinics.AddRangeAsync(toAdd);
            await SyncPatientAccountsAsync(patientId, newClinicIds);
        }

        // Ensures every account owning one of the patient's clinics has a PatientAccount membership.
        private async Task SyncPatientAccountsAsync(int patientId, List<int> clinicIds)
        {
            var accountIds = await _context.Clinics
                .Where(c => clinicIds.Contains(c.Id) && c.AccountId.HasValue)
                .Select(c => c.AccountId!.Value)
                .Distinct()
                .ToListAsync();

            if (accountIds.Count == 0)
            {
                await _context.SaveChangesAsync();
                return;
            }

            var hasPrimary = await _context.PatientAccounts
                .AnyAsync(pa => pa.PatientId == patientId && pa.IsPrimaryAccount && !pa.IsDeleted);

            foreach (var accountId in accountIds)
            {
                var exists = await _context.PatientAccounts
                    .FirstOrDefaultAsync(pa => pa.PatientId == patientId && pa.AccountId == accountId);

                if (exists == null)
                {
                    var isPrimary = !hasPrimary;
                    if (isPrimary) hasPrimary = true;

                    _context.PatientAccounts.Add(new PatientAccount
                    {
                        PatientId = patientId,
                        AccountId = accountId,
                        IsPrimaryAccount = isPrimary,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserBelongsToClinicAsync(int userId, int clinicId)
        {
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.ClinicId == clinicId && !u.IsDeleted);
            return user != null;
        }

        public async Task<int?> GetClinicAccountIdAsync(int clinicId)
        {
            return await _context.Clinics.AsNoTracking()
                .Where(c => c.Id == clinicId && c.AccountId.HasValue)
                .Select(c => c.AccountId!.Value)
                .FirstOrDefaultAsync();
        }

        public async Task CreatePatientAccountAsync(int patientId, int accountId, bool isPrimary, bool isVerifiedByPatient, bool? consentToShareProfile)
        {
            if (isPrimary)
            {
                var others = await _context.PatientAccounts
                    .Where(pa => pa.PatientId == patientId && pa.AccountId != accountId && pa.IsPrimaryAccount && !pa.IsDeleted)
                    .ToListAsync();
                foreach (var other in others)
                {
                    other.IsPrimaryAccount = false;
                    _context.PatientAccounts.Update(other);
                }
            }

            var existing = await _context.PatientAccounts
                .FirstOrDefaultAsync(pa => pa.PatientId == patientId && pa.AccountId == accountId);

            if (existing != null)
            {
                existing.IsVerifiedByPatient = isVerifiedByPatient;
                existing.ConsentToShareProfile = consentToShareProfile;
                if (isPrimary)
                    existing.IsPrimaryAccount = true;
                _context.PatientAccounts.Update(existing);
            }
            else
            {
                _context.PatientAccounts.Add(new PatientAccount
                {
                    PatientId = patientId,
                    AccountId = accountId,
                    IsPrimaryAccount = isPrimary,
                    IsVerifiedByPatient = isVerifiedByPatient,
                    ConsentToShareProfile = consentToShareProfile,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasVerifiedMembershipAsync(int patientId, int accountId)
        {
            return await _context.PatientAccounts.AsNoTracking()
                .AnyAsync(pa =>
                    pa.PatientId == patientId &&
                    pa.AccountId == accountId &&
                    pa.IsVerifiedByPatient &&
                    !pa.IsDeleted);
        }
    }
}
