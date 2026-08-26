using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using MedPal.API.Data;
using MedPal.API.Interfaces;
using MedPal.API.Models;
using MedPal.API.Models.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace MedPal.API.Interceptors
{
    public class AuditSaveChangesInterceptor : SaveChangesInterceptor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = false,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        // Entities to skip (recursion prevention and separate audit systems)
        private static readonly HashSet<string> SkippedEntityTypes = new()
        {
            nameof(AuditLog),
            nameof(MedicalRecordAccessLog),
            nameof(RoleAuditLog),
            nameof(NotificationMessage)
        };

        public AuditSaveChangesInterceptor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;
            if (context == null)
                return base.SavingChangesAsync(eventData, result, cancellationToken);

            var userId = GetUserId();
            var ipAddress = GetIpAddress();
            var userAgent = GetUserAgent();
            var now = DateTime.UtcNow;

            var auditEntries = new List<AuditLog>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity == null) continue;
                var typeName = entry.Entity.GetType().Name;

                if (SkippedEntityTypes.Contains(typeName))
                    continue;

                // Auto-populate IAuditableEntity fields
                if (entry.Entity is IAuditableEntity auditable)
                {
                    if (entry.State == EntityState.Added)
                    {
                        auditable.CreatedAt = now;
                        if (userId.HasValue)
                            auditable.CreatedByUserId = userId;
                    }

                    if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                    {
                        auditable.UpdatedAt = now;
                        auditable.LastModifiedAt = now;
                        if (userId.HasValue)
                        {
                            auditable.UpdatedByUserId = userId;
                            auditable.LastModifiedByUserId = userId;
                        }
                    }
                }

                // Auto-populate ISoftDelete fields
                if (entry.State == EntityState.Modified && entry.Entity is ISoftDelete softDelete)
                {
                    var isDeletedProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == nameof(ISoftDelete.IsDeleted));
                    if (isDeletedProp != null && isDeletedProp.IsModified && (bool)isDeletedProp.CurrentValue!)
                    {
                        softDelete.DeletedAt = now;
                        if (userId.HasValue)
                            softDelete.DeletedByUserId = userId;
                    }
                }

                // Build audit log entry
                var auditEntry = BuildAuditEntry(entry, typeName, userId, ipAddress, userAgent, now);
                if (auditEntry != null)
                    auditEntries.Add(auditEntry);
            }

            if (auditEntries.Count > 0 && userId.HasValue)
                context.Set<AuditLog>().AddRange(auditEntries);

            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private AuditLog BuildAuditEntry(
            Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry,
            string typeName,
            int? userId,
            string ipAddress,
            string userAgent,
            DateTime now)
        {
            var entityId = GetEntityId(entry);
            if (entityId == null)
                return null;

            string action;
            Dictionary<string, object> oldValues = null;
            Dictionary<string, object> newValues = null;
            List<string> changedFields = null;

            switch (entry.State)
            {
                case EntityState.Added:
                    action = "Create";
                    break;

                case EntityState.Modified:
                    changedFields = new List<string>();
                    oldValues = new Dictionary<string, object>();
                    newValues = new Dictionary<string, object>();

                    foreach (var prop in entry.Properties)
                    {
                        if (prop.IsModified)
                        {
                            // Skip audit-timestamp fields to reduce noise
                            if (prop.Metadata.Name is nameof(IAuditableEntity.UpdatedAt)
                                or nameof(IAuditableEntity.LastModifiedAt))
                                continue;

                            changedFields.Add(prop.Metadata.Name);
                            oldValues[prop.Metadata.Name] = prop.OriginalValue;
                            newValues[prop.Metadata.Name] = prop.CurrentValue;
                        }
                    }

                    // Detect soft delete
                    if (entry.Entity is ISoftDelete sd && changedFields.Contains(nameof(ISoftDelete.IsDeleted))
                        && (bool)entry.Properties.First(p => p.Metadata.Name == nameof(ISoftDelete.IsDeleted)).CurrentValue!)
                    {
                        action = "Delete";
                    }
                    else if (changedFields.Count == 0)
                    {
                        return null; // No meaningful changes
                    }
                    else
                    {
                        action = "Update";
                    }
                    break;

                case EntityState.Deleted:
                    action = "HardDelete";
                    break;

                default:
                    return null;
            }

            return new AuditLog
            {
                UserId = userId ?? 0,
                EntityType = typeName,
                EntityId = entityId.Value,
                Action = action,
                ChangedFields = changedFields?.Count > 0 ? string.Join(",", changedFields) : null,
                OldValues = oldValues?.Count > 0 ? JsonSerializer.Serialize(oldValues, JsonOptions) : null,
                NewValues = newValues?.Count > 0 ? JsonSerializer.Serialize(newValues, JsonOptions) : null,
                Timestamp = now,
                IpAddress = ipAddress,
                UserAgent = userAgent
            };
        }

        private int? GetEntityId(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry entry)
        {
            // Try the standard "Id" property
            var idProp = entry.Properties.FirstOrDefault(p => p.Metadata.Name == "Id");
            if (idProp != null && idProp.CurrentValue != null)
            {
                if (idProp.CurrentValue is int intId)
                    return intId;
                if (int.TryParse(idProp.CurrentValue.ToString(), out var parsedId))
                    return parsedId;
            }

            // For Added entities without a generated ID yet, return null
            return null;
        }

        private int? GetUserId()
        {
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null) return null;

            // Try "user_id" claim first (custom JWT), then standard nameidentifier
            var userIdClaim = user.FindFirst("user_id")?.Value
                ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (int.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }

        private string GetIpAddress()
        {
            return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        }

        private string GetUserAgent()
        {
            return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].FirstOrDefault();
        }
    }
}
