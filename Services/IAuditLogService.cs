using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Services
{
    public interface IAuditLogService
    {
        Task<List<AuditLog>> GetAuditLogsAsync(string entityType = null, int? entityId = null,
            int? userId = null, string action = null, int page = 1, int pageSize = 25);

        Task<int> GetAuditLogsCountAsync(string entityType = null, int? entityId = null,
            int? userId = null, string action = null);

        Task<AuditLog?> GetAuditLogByIdAsync(int id);
    }
}
