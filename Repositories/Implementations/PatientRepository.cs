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

            if (userId.HasValue)
            {
                query = query.Where(p => p.PatientDetails.MedicalHistories
                    .Any(mh => mh.HealthcareProfessionalId == userId.Value));
            }

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

        public async Task<Patient> GetPatientByIdAsync(int id)
        {
            var patient = await ApplyTenantFilter(_context.Patients)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (patient == null)
            {
                throw new KeyNotFoundException($"Patient with Id {id} not found.");
            }
            return patient;
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
            await _context.SaveChangesAsync();
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
            await _context.SaveChangesAsync();
        }

        public async Task<bool> UserBelongsToClinicAsync(int userId, int clinicId)
        {
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.ClinicId == clinicId && !u.IsDeleted);
            return user != null;
        }
    }
}
