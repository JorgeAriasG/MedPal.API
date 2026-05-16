using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class ClinicRepository : TenantAwareRepository<Clinic>, IClinicRepository
    {
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;

        public ClinicRepository(AppDbContext context, IMapper mapper, ITenantContextService tenantContext, IUserRepository userRepository)
            : base(context, tenantContext)
        {
            _mapper = mapper;
            _userRepository = userRepository;
        }

        public async Task<Clinic> GetClinicByIdAsync(int id)
        {
            var clinic = await _context.Clinics.AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == id);

            if (clinic == null)
            {
                throw new KeyNotFoundException($"Clinic with Id {id} not found.");
            }

            return clinic;
        }

        public async Task<Clinic> AddClinicAsync(int userId, Clinic clinic)
        {
            SetDate(clinic);
            await _context.Clinics.AddAsync(clinic);
            await _context.SaveChangesAsync();

            return clinic;
        }

        public async Task UpdateClinicAsync(Clinic clinic)
        {
            clinic.UpdatedAt = System.DateTime.Now;
            _context.Clinics.Update(clinic);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteClinicAsync(int id)
        {
            var clinic = await _context.Clinics.FindAsync(id);
            if (clinic == null)
            {
                throw new KeyNotFoundException($"Clinic with Id {id} not found.");
            }
            _context.Clinics.Remove(clinic);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ClinicExistsAsync(int id)
        {
            return await _context.Clinics.AnyAsync(c => c.Id == id);
        }

        public void DetachEntity<T>(T entity) where T : class
        {
            var entry = _context.Entry(entity);
            if (entry != null)
            {
                entry.State = EntityState.Detached;
            }
        }

        private void SetDate(Clinic clinic)
        {
            clinic.CreatedAt = System.DateTime.Now;
            clinic.UpdatedAt = System.DateTime.Now;
        }

        public async Task<bool> UserBelongsToClinicAsync(int userId, int clinicId)
        {
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && u.ClinicId == clinicId && !u.IsDeleted);
            return user != null;
        }

        public async Task<IEnumerable<Clinic>> GetAllClinicsAsync(int userId)
        {
            var user = await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user == null)
                return Enumerable.Empty<Clinic>();

            var clinic = await _context.Clinics
                .FirstOrDefaultAsync(c => c.Id == user.ClinicId && !c.IsDeleted);

            return clinic != null ? new[] { clinic } : Enumerable.Empty<Clinic>();
        }

        public async Task<IEnumerable<Clinic>> GetAllClinicsAsync()
        {
            return await _context.Clinics
                .Where(c => !c.IsDeleted)
                .ToListAsync();
        }
    }
}
