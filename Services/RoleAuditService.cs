using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedPal.API.Data;
using MedPal.API.Models.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedPal.API.Services
{
    /// <summary>
    /// Service for managing role audit logs
    /// Ensures all role operations are tracked for NOM-024 compliance
    /// </summary>
    public class RoleAuditService : IRoleAuditService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<RoleAuditService> _logger;

        public RoleAuditService(AppDbContext context, ILogger<RoleAuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogRoleAssignmentAsync(RoleAuditLog auditLog)
        {
            if (auditLog == null)
                throw new ArgumentNullException(nameof(auditLog));

            auditLog.Action = "Assigned";
            auditLog.Timestamp = DateTime.UtcNow;

            _context.RoleAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Role audit log created: User {UserId} assigned role {RoleId} by {AssignedBy} in clinic {ClinicId}. Reason: {Reason}",
                auditLog.UserId,
                auditLog.RoleId,
                auditLog.AssignedByUserId,
                auditLog.ClinicId,
                auditLog.Reason
            );
        }

        public async Task LogRoleRemovalAsync(RoleAuditLog auditLog)
        {
            if (auditLog == null)
                throw new ArgumentNullException(nameof(auditLog));

            auditLog.Action = "Removed";
            auditLog.Timestamp = DateTime.UtcNow;

            _context.RoleAuditLogs.Add(auditLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Role audit log created: User {UserId} removed role {RoleId} by {RemovedBy} in clinic {ClinicId}. Reason: {Reason}",
                auditLog.UserId,
                auditLog.RoleId,
                auditLog.AssignedByUserId,
                auditLog.ClinicId,
                auditLog.Reason
            );
        }

        public async Task<MedPal.API.DTOs.PagedResult<RoleAuditLog>> GetAuditLogsAsync(
            int? userId = null,
            int? clinicId = null,
            string? action = null,
            DateTime? fromDate = null,
            DateTime? toDate = null,
            int pageNumber = 1,
            int pageSize = 20)
        {
            // Validate pagination parameters
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100; // Max 100 items per page

            var query = _context.RoleAuditLogs
                .Include(a => a.User)
                .Include(a => a.Role)
                .Include(a => a.Clinic)
                .Include(a => a.AssignedByUser)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId);

            if (clinicId.HasValue)
                query = query.Where(a => a.ClinicId == clinicId);

            if (!string.IsNullOrWhiteSpace(action))
                query = query.Where(a => a.Action == action);

            if (fromDate.HasValue)
                query = query.Where(a => a.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Timestamp <= toDate.Value);

            // Get total count
            var totalCount = await query.CountAsync();

            // Calculate total pages
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var items = await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new MedPal.API.DTOs.PagedResult<RoleAuditLog>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };
        }
    }
}
