using MedPal.API.Data;
using MedPal.API.Models.Authorization;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Authorization
{
    public class PermissionRepository : IPermissionRepository
    {
        private readonly AppDbContext _context;

        public PermissionRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Permission?> GetPermissionByNameAsync(string name)
        {
            return await _context.Permissions
                .FirstOrDefaultAsync(p => p.Name == name);
        }

        public async Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(int roleId)
        {
            return await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.Permission)
                .ToListAsync();
        }

        public async Task<IEnumerable<Permission>> GetPermissionsForUserAsync(int userId, int? clinicId = null)
        {
            var query = _context.UserRoles
                .Where(ur => ur.UserId == userId)
                // Filter active roles (not expired)
                .Where(ur => !ur.ExpiresAt.HasValue || ur.ExpiresAt > DateTime.UtcNow);

            if (clinicId.HasValue)
            {
                // Include both clinic-specific and global roles
                query = query.Where(ur => ur.ClinicId == clinicId || ur.ClinicId == null);
            }

            var permissions = await query
                .SelectMany(ur => ur.Role.RolePermissions)
                .Select(rp => rp.Permission)
                .Distinct()
                .ToListAsync();

            return permissions;
        }

        public async Task GrantPermissionToRoleAsync(int roleId, int permissionId, int? grantedByUserId = null)
        {
            // Check if already exists
            var existing = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (existing != null)
                return;

            var rolePermission = new RolePermission
            {
                RoleId = roleId,
                PermissionId = permissionId,
                GrantedAt = DateTime.UtcNow,
                GrantedByUserId = grantedByUserId
            };

            _context.RolePermissions.Add(rolePermission);
            await _context.SaveChangesAsync();
        }

        public async Task RevokePermissionFromRoleAsync(int roleId, int permissionId)
        {
            var rolePermission = await _context.RolePermissions
                .FirstOrDefaultAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);

            if (rolePermission != null)
            {
                _context.RolePermissions.Remove(rolePermission);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> UserHasPermissionAsync(int userId, string permissionName, int? clinicId = null)
        {
            var query = _context.UserRoles
                .Where(ur => ur.UserId == userId)
                // Filter active roles
                .Where(ur => !ur.ExpiresAt.HasValue || ur.ExpiresAt > DateTime.UtcNow);

            if (clinicId.HasValue)
            {
                query = query.Where(ur => ur.ClinicId == clinicId || ur.ClinicId == null);
            }

            var hasPermission = await query
                .SelectMany(ur => ur.Role.RolePermissions)
                .AnyAsync(rp => rp.Permission.Name == permissionName);

            return hasPermission;
        }

        public async Task<Dictionary<string, List<Permission>>> GetPermissionsByResourceAsync()
        {
            var permissions = await _context.Permissions.ToListAsync();

            return permissions
                .GroupBy(p => p.Resource)
                .ToDictionary(
                    g => g.Key,
                    g => g.ToList()
                );
        }
    }
}
