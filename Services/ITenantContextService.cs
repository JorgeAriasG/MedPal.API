using MedPal.API.Models;
using MedPal.API.Enums;

namespace MedPal.API.Services;

/// <summary>
/// Servicio para acceder al contexto de tenancy del usuario actual.
/// Proporciona información sobre la cuenta, clínica, rol y permisos del usuario.
/// </summary>
public interface ITenantContextService
{
    /// <summary>
    /// ID de la cuenta actual del usuario.
    /// Null si el usuario no está autenticado o no tiene cuenta asignada.
    /// </summary>
    int? CurrentAccountId { get; }

    /// <summary>
    /// ID de la clínica actual del usuario (su clínica principal).
    /// Null si el usuario es SuperAdmin o no tiene clínica asignada.
    /// </summary>
    int? CurrentClinicId { get; }

    /// <summary>
    /// ID del usuario actual autenticado.
    /// Null si el usuario no está autenticado.
    /// </summary>
    int? CurrentUserId { get; }

    /// <summary>
    /// Rol del usuario actual en el sistema.
    /// Null si el usuario no está autenticado.
    /// </summary>
    SystemRole? CurrentRole { get; }

    /// <summary>
    /// Indica si el usuario actual es SuperAdmin.
    /// SuperAdmin tiene acceso a todos los datos del sistema.
    /// </summary>
    bool IsSuperAdmin { get; }

    /// <summary>
    /// Indica si el usuario actual es AccountAdmin.
    /// AccountAdmin tiene acceso a toda la cuenta (todas sus clínicas).
    /// </summary>
    bool IsAccountAdmin { get; }

    /// <summary>
    /// Indica si el usuario actual es ClinicAdmin.
    /// ClinicAdmin tiene acceso solo a su clínica.
    /// </summary>
    bool IsClinicAdmin { get; }

    /// <summary>
    /// Valida si el usuario tiene acceso a una clínica específica.
    /// </summary>
    /// <param name="clinicId">ID de la clínica a validar</param>
    /// <returns>True si el usuario tiene acceso a la clínica; false en caso contrario</returns>
    Task<bool> HasAccessToClinicAsync(int clinicId);

    /// <summary>
    /// Valida si el usuario tiene acceso a una cuenta específica.
    /// </summary>
    /// <param name="accountId">ID de la cuenta a validar</param>
    /// <returns>True si el usuario tiene acceso a la cuenta; false en caso contrario</returns>
    Task<bool> HasAccessToAccountAsync(int accountId);

    /// <summary>
    /// Obtiene el rol del usuario como enumeración SystemRole.
    /// </summary>
    /// <returns>El rol del usuario o null si no está autenticado</returns>
    SystemRole? GetUserRole();

    /// <summary>
    /// Obtiene el ID del usuario actual.
    /// </summary>
    /// <returns>ID del usuario o null si no está autenticado</returns>
    int? GetUserId();

    /// <summary>
    /// Obtiene el ID de la cuenta actual.
    /// </summary>
    /// <returns>ID de la cuenta o null si el usuario no tiene cuenta</returns>
    int? GetAccountId();

    /// <summary>
    /// Obtiene el ID de la clínica actual (principal).
    /// </summary>
    /// <returns>ID de la clínica o null si el usuario no tiene clínica</returns>
    int? GetClinicId();
}
