using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedPal.API.Data;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Services
{
    public class AuditLogService : IAuditLogService
    {
        private readonly AppDbContext _context;

        public AuditLogService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLog>> GetAuditLogsAsync(
            string entityType = null, int? entityId = null,
            int? userId = null, string action = null,
            int page = 1, int pageSize = 25)
        {
            var query = _context.AuditLogs
                .Include(a => a.User)
                .IgnoreQueryFilters()
                .AsQueryable();

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (entityId.HasValue)
                query = query.Where(a => a.EntityId == entityId.Value);

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetAuditLogsCountAsync(
            string entityType = null, int? entityId = null,
            int? userId = null, string action = null)
        {
            var query = _context.AuditLogs.AsQueryable();

            if (!string.IsNullOrEmpty(entityType))
                query = query.Where(a => a.EntityType == entityType);

            if (entityId.HasValue)
                query = query.Where(a => a.EntityId == entityId.Value);

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId.Value);

            if (!string.IsNullOrEmpty(action))
                query = query.Where(a => a.Action == action);

            return await query.CountAsync();
        }

        public async Task<AuditLog?> GetAuditLogByIdAsync(int id)
        {
            return await _context.AuditLogs
                .Include(a => a.User)
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.Id == id);
        }
    }
}
