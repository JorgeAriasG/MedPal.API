using MedPal.API.Models.Authorization;

namespace MedPal.API.Repositories.Authorization
{
    /// <summary>
    /// Repository interface for Role management operations
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>
        /// Get a role by its name
        /// </summary>
        Task<Role?> GetRoleByNameAsync(string name);

        /// <summary>
        /// Get a role by its ID
        /// </summary>
        /// <param name="roleId">Role ID</param>
        /// <returns>Role if found, null otherwise</returns>
        Task<Role?> GetRoleByIdAsync(int roleId);

        /// <summary>
        /// Get all active roles with their permissions
        /// </summary>
        Task<IEnumerable<Role>> GetRolesWithPermissionsAsync();

        /// <summary>
        /// Assign a role to a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="roleId">Role ID</param>
        /// <param name="clinicId">Clinic ID for scoped roles (null for global)</param>
        /// <param name="expiresAt">Optional expiration date for temporal roles</param>
        /// <param name="assignedByUserId">User who assigned this role</param>
        Task AssignRoleToUserAsync(int userId, int roleId, int? clinicId = null, DateTime? expiresAt = null, int? assignedByUserId = null);

        /// <summary>
        /// Remove a role from a user
        /// </summary>
        Task RemoveRoleFromUserAsync(int userId, int roleId, int? clinicId = null);

        /// <summary>
        /// Get all roles assigned to a user
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="clinicId">Clinic ID to filter scoped roles (null for all)</param>
        Task<IEnumerable<UserRole>> GetUserRolesAsync(int userId, int? clinicId = null);

        /// <summary>
        /// Check if a user has a specific role
        /// </summary>
        Task<bool> UserHasRoleAsync(int userId, string roleName, int? clinicId = null);
    }
}
