using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.Data;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class ClinicRepository : IClinicRepository
    {
        private readonly AppDbContext _context;
        private readonly IMapper _mapper;

        public ClinicRepository(AppDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<IEnumerable<Clinic>> GetAllClinicsAsync(int id)
        {
            return await _context.Clinics
                .Where(c => c.UserClinics.Any(uc => uc.UserId == id))
                .ToListAsync();
        }

        public async Task<Clinic> GetClinicByIdAsync(int id)
        {
            var clinic = await _context.Clinics.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);

            if (clinic == null)
            {
                throw new KeyNotFoundException($"Clinic with Id {id} not found.");
            }

            return clinic;
        }

        public async Task<UserClinic> GetUserClinicByIdAsync(int id)
        {
            var clinic = await _context.UserClinics.FirstOrDefaultAsync(c => c.UserId == id);
            return clinic;
        }

        public async Task<Clinic> AddClinicAsync(int userId, Clinic clinic)
        {
            SetDate(clinic);
            await _context.Clinics.AddAsync(clinic);
            await _context.SaveChangesAsync();

            var userClinic = new UserClinic
            {
                UserId = userId,
                ClinicId = clinic.Id
            };

            await AddUserClinicAsync(userClinic);

            return clinic;
        }

        public async Task<UserClinic> AddUserClinicAsync(UserClinic userClinic)
        {
            await _context.UserClinics.AddAsync(userClinic);
            await _context.SaveChangesAsync();
            return userClinic;
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
            return await _context.UserClinics
                .AnyAsync(uc => uc.UserId == userId && uc.ClinicId == clinicId);
        }
    }
}