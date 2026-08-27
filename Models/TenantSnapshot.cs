using System;

namespace MedPal.API.Models;

/// <summary>
/// Immutable snapshot of the current tenant context, extracted from JWT claims per-request.
/// Used by AppDbContext query filters to enforce multi-tenant data isolation.
///
/// This record is NOT a service — it has no dependencies on AppDbContext or any EF Core types.
/// It is constructed fresh for each Scoped DbContext instance from IHttpContextAccessor.
///
/// EF Core evaluates DbContext instance fields per-query, so each request sees its own snapshot.
/// </summary>
public sealed record TenantSnapshot
{
    public int? AccountId { get; init; }
    public int? ClinicId { get; init; }
    public int? UserId { get; init; }
    public string? Role { get; init; }

    public bool IsSuperAdmin =>
        string.Equals(Role, "SuperAdmin", StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// True when an authenticated STAFF user exists (UserId is set).
    /// False during migrations, seeders, background jobs without HTTP context,
    /// for anonymous callers, and for patient portal tokens (patients are not
    /// tenant principals - their data is scoped by explicit patientId filters).
    /// </summary>
    public bool HasContext => UserId != null;
}
