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
            // Aplicar filtro de tenant: SuperAdmin ve todos, otros ven solo su AccountId
            return await ApplyTenantFilter(_context.Users.Where(u => !u.IsDeleted))
                .ToListAsync();
        }

        public async Task<User> GetUserByIdAsync(int id)
        {
            var query = _context.Users.AsQueryable();
            
            // Aplicar filtro de tenant: SuperAdmin ve todos, otros ven solo su AccountId
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

        public async Task<List<User>> GetAllUsersByAccountId(int accountId)
        {
            // SuperAdmin puede ver usuarios de cualquier AccountId
            // AccountAdmin solo puede ver su propio AccountId
            if (!_tenantContext.IsSuperAdmin && _tenantContext.CurrentAccountId != accountId)
            {
                return new List<User>();  // No tiene permiso
            }

            return await _context.Users
                .Where(u => u.AccountId == accountId && !u.IsDeleted)
                .ToListAsync();
        }

        /// <summary>
        /// Obtiene el perfil del usuario autenticado ignorando filtros multi-tenancy.
        /// Se usa para que un usuario pueda acceder a SU PROPIO perfil después de login.
        /// Validación de seguridad: Solo devuelve el usuario si no está eliminado.
        /// </summary>
        public async Task<User> GetOwnProfileAsync(int userId)
        {
            return await _context.Users
                .IgnoreQueryFilters()  // Ignorar filtros multi-tenancy para acceso a self-profile
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
        }

        public async Task<User> GetUserByEmailAsync(string email)
        {
            // Normalizar email: trim y convertir a minúsculas para búsqueda case-insensitive
            var normalizedEmail = email?.Trim().ToLower() ?? string.Empty;
            return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
        }

        public async Task<User> AddUserAsync(User user)
        {
            // Hash the password before saving the user
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash);

            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(int id, User user)
        {
            var existingUser = await _context.Users.FindAsync(id);
            if (existingUser == null)
            {
                throw new KeyNotFoundException($"User with Id {id} not found.");
            }
            _mapper.Map(user, existingUser);
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
            // Normalizar email: trim y convertir a minúsculas
            var normalizedEmail = email?.Trim().ToLower() ?? string.Empty;
            
            // ⚠️ IMPORTANTE: Usar IgnoreQueryFilters() porque durante login no hay contexto de tenant definido
            // El query filter de AccountId filtraría incorrectamente los usuarios
            var user = await _context.Users
                .IgnoreQueryFilters()  // Ignorar filtros globales de tenant para encontrar el usuario
                .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
                .SingleOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);
            
            if (user == null)
            {
                Console.WriteLine($"[LOGIN DEBUG] Usuario no encontrado con email: {normalizedEmail}");
                return null;
            }

            // Validar contraseña con BCrypt
            bool passwordValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
            
            if (!passwordValid)
            {
                Console.WriteLine($"[LOGIN DEBUG] Contraseña inválida para usuario: {normalizedEmail}");
                Console.WriteLine($"[LOGIN DEBUG] PasswordHash en BD comienza con: {user.PasswordHash.Substring(0, 7)}...");
                return null;
            }

            // Validar que el usuario esté activo
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

        // Soft delete: marcar usuario como eliminado sin borrar física
        public async Task SoftDeleteUserAsync(int userId, int deletedByUserId)
        {
            var user = await GetUserByIdAsync(userId);

            user.IsDeleted = true;
            user.IsActive = false;
            user.DeletedAt = DateTime.UtcNow;
            user.DeactivatedByUserId = deletedByUserId;
            user.UpdatedAt = DateTime.UtcNow;

            await UpdateUserAsync(userId, user);
        }

        // Restaurar usuario (si es necesario)
        public async Task RestoreUserAsync(int userId)
        {
            var user = await GetUserByIdAsync(userId);

            user.IsDeleted = false;
            user.IsActive = true;
            user.DeletedAt = null;
            user.DeactivatedByUserId = null;
            user.UpdatedAt = DateTime.UtcNow;

            await UpdateUserAsync(userId, user);
        }

        public async Task UpdateUserLastAccessAtAsync(int userId)
        {
            // ⚠️ Usar IgnoreQueryFilters() porque se ejecuta durante login cuando el contexto de tenant aún no está disponible
            var user = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);
            
            if (user != null)
            {
                user.LastAccessAt = DateTime.UtcNow;
                await UpdateUserAsync(userId, user);
            }
        }

        /// <summary>
        /// Obtiene los roles activos de un usuario (excluyendo eliminados).
        /// </summary>
        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            var userRoles = await _context.UserRoles
                .Where(ur => ur.UserId == userId && !ur.IsDeleted)
                .Include(ur => ur.Role)
                .Select(ur => ur.Role.Name)
                .ToListAsync();

            return userRoles;
        }

        /// <summary>
        /// Obtiene las clínicas asociadas a un usuario (excluyendo eliminadas).
        /// </summary>
        public async Task<List<Clinic>> GetUserClinicsAsync(int userId)
        {
            var userClinics = await _context.UserClinics
                .Where(uc => uc.UserId == userId && !uc.IsDeleted)
                .Include(uc => uc.Clinic)
                .Select(uc => new Clinic
                {
                    Id = uc.ClinicId,
                    Name = uc.Clinic.Name ?? "Clínica Desconocida"
                })
                .ToListAsync();

            return userClinics;
        }
    }
}