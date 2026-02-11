using System.Security.Claims;
using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Services.Implementations;

/// <summary>
/// Implementación de ITenantContextService.
/// Extrae y proporciona información de tenancy del usuario autenticado actual.
/// </summary>
public class TenantContextService : ITenantContextService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly AppDbContext _dbContext;
    private readonly ILogger<TenantContextService> _logger;

    // Valores cacheados para evitar extracciones múltiples en el mismo request
    private int? _cachedAccountId;
    private int? _cachedClinicId;
    private int? _cachedUserId;
    private SystemRole? _cachedRole;
    private bool _isCached = false;

    public TenantContextService(
        IHttpContextAccessor httpContextAccessor,
        AppDbContext dbContext,
        ILogger<TenantContextService> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// Extrae y cachea los claims del usuario autenticado.
    /// </summary>
    private void EnsureCached()
    {
        if (_isCached)
            return;

        var user = _httpContextAccessor.HttpContext?.User;
        if (user == null || !(user.Identity?.IsAuthenticated ?? false))
        {
            _logger.LogWarning("Intento de acceso a TenantContext sin usuario autenticado");
            _isCached = true;
            return;
        }

        // Extraer account_id del claim
        var accountIdClaim = user.FindFirst("account_id");
        if (int.TryParse(accountIdClaim?.Value, out var accountId))
        {
            _cachedAccountId = accountId;
        }

        // Extraer clinic_id del claim
        var clinicIdClaim = user.FindFirst("clinic_id");
        if (int.TryParse(clinicIdClaim?.Value, out var clinicId))
        {
            _cachedClinicId = clinicId;
        }

        // Extraer user_id del claim
        var userIdClaim = user.FindFirst("user_id") ?? user.FindFirst(ClaimTypes.NameIdentifier);
        if (int.TryParse(userIdClaim?.Value, out var userId))
        {
            _cachedUserId = userId;
        }

        // Extraer role del claim
        var roleClaim = user.FindFirst("role") ?? user.FindFirst(ClaimTypes.Role);
        if (roleClaim?.Value != null && !string.IsNullOrEmpty(roleClaim.Value))
        {
            if (Enum.TryParse<SystemRole>(roleClaim.Value, ignoreCase: true, out var role))
            {
                _cachedRole = role;
            }
            else
            {
                _logger.LogWarning($"No se pudo parsear rol: {roleClaim.Value}");
            }
        }

        _isCached = true;

    }

    public int? CurrentAccountId
    {
        get
        {
            EnsureCached();
            return _cachedAccountId;
        }
    }

    public int? CurrentClinicId
    {
        get
        {
            EnsureCached();
            return _cachedClinicId;
        }
    }

    public int? CurrentUserId
    {
        get
        {
            EnsureCached();
            return _cachedUserId;
        }
    }

    public SystemRole? CurrentRole
    {
        get
        {
            EnsureCached();
            return _cachedRole;
        }
    }

    public bool IsSuperAdmin
    {
        get
        {
            EnsureCached();
            return _cachedRole == SystemRole.SuperAdmin;
        }
    }

    public bool IsAccountAdmin
    {
        get
        {
            EnsureCached();
            return _cachedRole == SystemRole.AccountAdmin;
        }
    }

    public bool IsClinicAdmin
    {
        get
        {
            EnsureCached();
            return _cachedRole == SystemRole.ClinicAdmin;
        }
    }

    public SystemRole? GetUserRole()
    {
        EnsureCached();
        return _cachedRole;
    }

    public int? GetUserId()
    {
        EnsureCached();
        return _cachedUserId;
    }

    public int? GetAccountId()
    {
        EnsureCached();
        return _cachedAccountId;
    }

    public int? GetClinicId()
    {
        EnsureCached();
        return _cachedClinicId;
    }

    /// <summary>
    /// Valida si el usuario tiene acceso a una clínica específica.
    /// </summary>
    public async Task<bool> HasAccessToClinicAsync(int clinicId)
    {
        EnsureCached();

        // SuperAdmin tiene acceso a todo
        if (IsSuperAdmin)
            return true;

        // Si no tenemos clínica en el contexto, validar que existe y pertenece a nuestra account
        if (_cachedClinicId == null)
        {
            _logger.LogWarning($"Usuario sin clínica intenta acceder a clínica {clinicId}");
            return false;
        }

        // ClinicAdmin/Doctor solo pueden acceder a su propia clínica
        if (_cachedClinicId == clinicId)
            return true;

        // AccountAdmin puede acceder a cualquier clínica de su cuenta
        if (IsAccountAdmin && _cachedAccountId != null)
        {
            var clinic = await _dbContext.Clinics
                .Where(c => c.Id == clinicId && c.AccountId == _cachedAccountId)
                .FirstOrDefaultAsync();
            
            return clinic != null;
        }

        _logger.LogWarning($"Usuario acceso denegado a clínica {clinicId}");
        return false;
    }

    /// <summary>
    /// Valida si el usuario tiene acceso a una cuenta específica.
    /// </summary>
    public async Task<bool> HasAccessToAccountAsync(int accountId)
    {
        EnsureCached();

        // SuperAdmin tiene acceso a todo
        if (IsSuperAdmin)
            return true;

        // AccountAdmin solo puede acceder a su propia cuenta
        if (IsAccountAdmin && _cachedAccountId == accountId)
            return true;

        // ClinicAdmin/Doctor pueden acceder a la cuenta a través de su clínica
        if (_cachedClinicId != null)
        {
            var clinic = await _dbContext.Clinics
                .Where(c => c.Id == _cachedClinicId && c.AccountId == accountId)
                .FirstOrDefaultAsync();

            if (clinic != null)
                return true;
        }

        _logger.LogWarning($"Usuario acceso denegado a cuenta {accountId}");
        return false;
    }
}
