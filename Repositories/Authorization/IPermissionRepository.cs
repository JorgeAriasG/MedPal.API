using MedPal.API.Models.Authorization;

namespace MedPal.API.Repositories.Authorization
{
    /// <summary>
    /// Repository interface for Permission management operations
    /// </summary>
    public interface IPermissionRepository
    {
        /// <summary>
        /// Get a permission by its name
        /// </summary>
        Task<Permission?> GetPermissionByNameAsync(string name);

        /// <summary>
        /// Get all permissions assigned to a role
        /// </summary>
        Task<IEnumerable<Permission>> GetPermissionsForRoleAsync(int roleId);

        /// <summary>
        /// Get all permissions a user has (through their roles)
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="clinicId">Clinic ID to filter by context (null for global)</param>
        Task<IEnumerable<Permission>> GetPermissionsForUserAsync(int userId, int? clinicId = null);

        /// <summary>
        /// Grant a permission to a role
        /// </summary>
        Task GrantPermissionToRoleAsync(int roleId, int permissionId, int? grantedByUserId = null);

        /// <summary>
        /// Revoke a permission from a role
        /// </summary>
        Task RevokePermissionFromRoleAsync(int roleId, int permissionId);

        /// <summary>
        /// Check if a user has a specific permission
        /// </summary>
        Task<bool> UserHasPermissionAsync(int userId, string permissionName, int? clinicId = null);

        /// <summary>
        /// Get all available permissions grouped by resource
        /// </summary>
        Task<Dictionary<string, List<Permission>>> GetPermissionsByResourceAsync();
    }
}
