using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedPal.API.DTOs;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/entity-audit-logs")]
    [Authorize(Policy = "ViewAuditLogPolicy")]
    public class EntityAuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;
        private readonly ILogger<EntityAuditLogController> _logger;

        public EntityAuditLogController(
            IAuditLogService auditLogService,
            ILogger<EntityAuditLogController> logger)
        {
            _auditLogService = auditLogService;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/entity-audit-logs — paginated entity change logs with filters
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAuditLogs(
            [FromQuery] string entityType = null,
            [FromQuery] int? entityId = null,
            [FromQuery] int? userId = null,
            [FromQuery] string action = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25)
        {
            var logs = await _auditLogService.GetAuditLogsAsync(entityType, entityId, userId, action, page, pageSize);
            var totalItems = await _auditLogService.GetAuditLogsCountAsync(entityType, entityId, userId, action);

            var dtos = logs.Select(MapToDto).ToList();

            return Ok(new AuditLogPagedResponseDto
            {
                Data = dtos,
                Pagination = new AuditLogPaginationDto
                {
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalItems,
                    TotalPages = (int)Math.Ceiling((double)totalItems / pageSize)
                }
            });
        }

        /// <summary>
        /// GET /api/entity-audit-logs/{id} — single audit log detail
        /// </summary>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuditLogById(int id)
        {
            var log = await _auditLogService.GetAuditLogByIdAsync(id);
            if (log == null)
                return NotFound();

            return Ok(MapToDto(log));
        }

        private static AuditLogReadDto MapToDto(Models.AuditLog log)
        {
            return new AuditLogReadDto
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = log.User?.Name,
                EntityType = log.EntityType,
                EntityId = log.EntityId,
                Action = log.Action,
                ChangedFields = log.ChangedFields,
                OldValues = log.OldValues,
                NewValues = log.NewValues,
                Timestamp = log.Timestamp,
                IpAddress = log.IpAddress,
                UserAgent = log.UserAgent,
                Description = log.Description
            };
        }
    }
}
