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

        public async Task<IEnumerable<Patient>> GetAllPatientsAsync(int clinicId, string? search = null, string? sortBy = "name", bool descending = false)
        {
            IQueryable<Patient> query = _context.Patients
                .Where(p => p.ClinicId == clinicId);

            // Searching
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();
                query = query.Where(p => 
                    p.Name.ToLower().Contains(search) || 
                    p.Lastname.ToLower().Contains(search) || 
                    p.Phone.Contains(search));
            }

            // Sorting
            switch (sortBy?.ToLower())
            {
                case "lastname":
                    query = descending ? query.OrderByDescending(p => p.Lastname) : query.OrderBy(p => p.Lastname);
                    break;
                case "createdat":
                    // Assuming you have a CreatedAt field, if not, use Id as proxy for now
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
                // Handle the case where the patient is not found
                throw new KeyNotFoundException($"Patient with Id {id} not found.");
            }
            return patient;
        }

        public async Task<Patient> AddPatientAsync(Patient patient)
        {
            // Inicializar detalles del paciente para evitar 404 en el frontend
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
    }
}