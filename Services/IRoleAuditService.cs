using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models.Authorization;

namespace MedPal.API.Services
{
    /// <summary>
    /// Service interface for role audit logging operations
    /// </summary>
    public interface IRoleAuditService
    {
        /// <summary>
        /// Log a role assignment operation
        /// </summary>
        /// <param name="auditLog">Audit log entry with assignment details</param>
        Task LogRoleAssignmentAsync(RoleAuditLog auditLog);

        /// <summary>
        /// Log a role removal operation
        /// </summary>
        /// <param name="auditLog">Audit log entry with removal details</param>
        Task LogRoleRemovalAsync(RoleAuditLog auditLog);

        /// <summary>
        /// Retrieve audit logs with optional filtering
        /// </summary>
        /// <param name="userId">Filter by user who received/lost roles</param>
        /// <param name="clinicId">Filter by clinic context</param>
        /// <param name="fromDate">Filter by start date (inclusive)</param>
        /// <param name="toDate">Filter by end date (inclusive)</param>
        /// <returns>Filtered audit logs ordered by timestamp descending</returns>
        /// <summary>
        /// Retrieve audit logs with pagination and filters
        /// </summary>
        /// <param name="userId">Filter by user who received/lost roles</param>
        /// <param name="clinicId">Filter by clinic context</param>
        /// <param name="action">Filter by action type (Assignment/Removal)</param>
        /// <param name="fromDate">Filter by start date (inclusive)</param>
        /// <param name="toDate">Filter by end date (inclusive)</param>
        /// <param name="pageNumber">Page number (1-indexed)</param>
        /// <param name="pageSize">Number of items per page</param>
        /// <returns>Paged result of audit logs</returns>
        Task<MedPal.API.DTOs.PagedResult<RoleAuditLog>> GetAuditLogsAsync(
            int? userId = null,
            int? clinicId = null,
            string? action = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 20
        );
    }
}
