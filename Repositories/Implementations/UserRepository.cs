using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class UserRepository : TenantAwareRepository<User>, IUserRepository
    {
        private readonly IMapper _mapper;

        public UserRepository(AppDbContext context, IMapper mapper, ITenantContextService tenantContext) 
            : base(context, tenantContext)
        {
            _mapper = mapper;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync()
        {
            return await ApplyTenantFilter(_context.Users.Where(u => !u.IsDeleted))
                .ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var query = _context.Users.AsQueryable();
            
            if (!_tenantContext.IsSuperAdmin && typeof(User).GetProperty("AccountId") != null)
            {
                query = query.Where(u => u.AccountId == _tenantContext.CurrentAccountId);
            }
            
            var user = await query.FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with Id {id} not found.");
            }
            return user;
        }

        public async Task<User?> GetByIdIgnoreTenantAsync(int id)
        {
            return await _context.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id && !u.IsDeleted);
        }

        public async Task<List<User>> GetAllUsersByAccountId(int accountId)
        {
            if (!_tenantContext.IsSuperAdmin && _tenantContext.CurrentAccountId != accountId)
            {
                return new List<User>();
            }

            return await _context.Users
                .Where(u => u.AccountId == accountId && !u.IsDeleted)
                .ToListAsync();
        }

        public async Task<User> GetOwnProfileAsync(int userId)
        {
            return await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            var normalizedEmail = email?.Trim().ToLower() ?? string.Empty;
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        }

        public async Task<User> AddUserAsync(User user)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException($"User with Id {user.Id} not found.");
            }
            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                throw new KeyNotFoundException($"User with Id {id} not found.");
            }
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> ValidateUserAsync(string email, string password)
        {
            var normalizedEmail = email?.Trim().ToLower() ?? string.Empty;
            
            var user = await _context.Users
                .IgnoreQueryFilters()
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            
            if (user == null)
            {
                Console.WriteLine($"[LOGIN DEBUG] Usuario no encontrado con email: {normalizedEmail}");
                return null;
            }

            bool passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            
            if (!passwordValid)
            {
                Console.WriteLine($"[LOGIN DEBUG] Contraseña inválida para usuario: {normalizedEmail}");
                return null;
            }

            if (!user.IsActive)
            {
                Console.WriteLine($"[LOGIN DEBUG] Usuario inactivo: {normalizedEmail}");
                return null;
            }

            if (user.IsDeleted)
            {
                Console.WriteLine($"[LOGIN DEBUG] Usuario eliminado: {normalizedEmail}");
                return null;
            }

            Console.WriteLine($"[LOGIN DEBUG] ✅ Login exitoso para: {normalizedEmail}");
            return user;
        }

        public async Task SoftDeleteUserAsync(int userId, int deletedByUserId)
        {
            var user = await GetUserByIdAsync(userId);
            user.IsDeleted = true;
            user.IsActive = false;
            user.DeletedAt = DateTime.UtcNow;
            user.DeactivatedByUserId = deletedByUserId;
            user.UpdatedAt = DateTime.UtcNow;
            await UpdateUserAsync(user);
        }

        public async Task RestoreUserAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);
            user.IsDeleted = false;
            user.IsActive = true;
            user.DeletedAt = null;
            user.DeactivatedByUserId = null;
            user.UpdatedAt = DateTime.UtcNow;
            await UpdateUserAsync(user);
        }

        public async Task UpdateUserLastAccessAtAsync(int userId)
        {
            var user = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            
            if (user != null)
            {
                user.LastAccessAt = DateTime.UtcNow;
                await UpdateUserAsync(user);
            }
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            return userRoles;
        }

        public async Task<Clinic?> GetUserClinicAsync(int userId)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

            if (user?.ClinicId == null)
                return null;

            return await _context.Clinics
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == user.ClinicId && !c.IsDeleted);
        }

        public async Task<List<User>> GetDoctorsByClinicAsync(int clinicId)
        {
            return await _context.Users
                .Where(u => !u.IsDeleted && u.ClinicId == clinicId)
                .Where(u => u.UserRoles.Any(ur => ur.Role.Name == "HealthProfessional" && !ur.IsDeleted))
                .ToListAsync();
        }
    }
}
