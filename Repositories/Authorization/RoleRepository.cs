using MedPal.API.Data;
using MedPal.API.Models.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Authorization
{
    public class RoleRepository : IRoleRepository
    {
        private readonly AppDbContext _context;

        public RoleRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Role?> GetRoleByNameAsync(string name)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Name == name && !r.IsDeleted);
        }

        public async Task<Role?> GetRoleByIdAsync(int roleId)
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .FirstOrDefaultAsync(r => r.Id == roleId && !r.IsDeleted);
        }

        public async Task<IEnumerable<Role>> GetRolesWithPermissionsAsync()
        {
            return await _context.Roles
                .Include(r => r.RolePermissions)
                    .ThenInclude(rp => rp.Permission)
                .Where(r => r.IsActive && !r.IsDeleted)
                .ToListAsync();
        }

        public async Task AssignRoleToUserAsync(int userId, int roleId, int? clinicId = null, DateTime? expiresAt = null, int? assignedByUserId = null)
        {
            // Check if assignment already exists
            var existingAssignment = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.ClinicId == clinicId);

            if (existingAssignment != null)
            {
                // Update expiration if needed
                if (expiresAt.HasValue)
                {
                    existingAssignment.ExpiresAt = expiresAt;
                    await _context.SaveChangesAsync();
                }
                return;
            }

            var userRole = new UserRole
            {
                UserId = userId,
                RoleId = roleId,
                ClinicId = clinicId,
                AssignedAt = DateTime.UtcNow,
                ExpiresAt = expiresAt,
                AssignedByUserId = assignedByUserId
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveRoleFromUserAsync(int userId, int roleId, int? clinicId = null)
        {
            var userRole = await _context.UserRoles
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == roleId && ur.ClinicId == clinicId);

            if (userRole != null)
            {
                _context.UserRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<UserRole>> GetUserRolesAsync(int userId, int? clinicId = null)
        {
            var query = _context.UserRoles
                .Include(ur => ur.Role)
                .Include(ur => ur.Clinic)
                .Where(ur => ur.UserId == userId);

            // Filter active roles (not expired)
            query = query.Where(ur => !ur.ExpiresAt.HasValue || ur.ExpiresAt > DateTime.UtcNow);

            if (clinicId.HasValue)
            {
                // Get roles for specific clinic or global roles
                query = query.Where(ur => ur.ClinicId == clinicId || ur.ClinicId == null);
            }

            return await query.ToListAsync();
        }

        public async Task<bool> UserHasRoleAsync(int userId, string roleName, int? clinicId = null)
        {
            var query = _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId && ur.Role.Name == roleName);

            // Filter active roles
            query = query.Where(ur => !ur.ExpiresAt.HasValue || ur.ExpiresAt > DateTime.UtcNow);

            if (clinicId.HasValue)
            {
                query = query.Where(ur => ur.ClinicId == clinicId || ur.ClinicId == null);
            }

            return await query.AnyAsync();
        }
    }
}
