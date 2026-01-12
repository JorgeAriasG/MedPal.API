using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models.Authorization;
using MedPal.API.Repositories;
using MedPal.API.Repositories.Authorization;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MedPal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : BaseController
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;
        private readonly IRoleAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IClinicRepository _clinicRepository;
        private readonly ILogger<RoleController> _logger;

        public RoleController(
            IRoleRepository roleRepository,
            IMapper mapper,
            IRoleAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            IClinicRepository clinicRepository,
            ILogger<RoleController> logger)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _clinicRepository = clinicRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get all active roles with their permissions
        /// </summary>
        [HttpGet]
        [Authorize(Policy = "Roles.View")]
        public async Task<ActionResult<IEnumerable<RoleReadDTO>>> GetAllRolesWithPermissions()
        {
            try
            {
                var roles = await _roleRepository.GetRolesWithPermissionsAsync();
                var roleReadDTOs = _mapper.Map<IEnumerable<RoleReadDTO>>(roles);
                return Ok(roleReadDTOs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving roles with permissions");
                return StatusCode(500, new { message = "Error interno del servidor. Por favor, contacte al administrador." });
            }
        }

        /// <summary>
        /// Get a role by name
        /// </summary>
        [HttpGet("by-name/{name}")]
        [Authorize(Policy = "Roles.View")]
        public async Task<ActionResult<RoleReadDTO>> GetRoleByName(string name)
        {
            try
            {
                var role = await _roleRepository.GetRoleByNameAsync(name);
                if (role == null)
                {
                    _logger.LogWarning("Role not found: {RoleName}", name);
                    return NotFound(new { message = $"Role with name '{name}' not found." });
                }

                var roleReadDTO = _mapper.Map<RoleReadDTO>(role);
                return Ok(roleReadDTO);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving role by name: {RoleName}", name);
                return StatusCode(500, new { message = "Error interno del servidor. Por favor, contacte al administrador." });
            }
        }

        /// <summary>
        /// Assign a role to a user (optionally scoped to a clinic)
        /// </summary>
        [HttpPost("{roleId}/assign-to-user")]
        [Authorize(Policy = "Roles.Assign")]
        public async Task<IActionResult> AssignRoleToUser(int roleId, [FromBody] AssignRoleDTO assignRoleDto)
        {
            try
            {
                // Get the ID of the user performing this action
                var assignedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(assignedByUserId, out int assignedById))
                {
                    _logger.LogWarning("Unauthorized role assignment attempt - could not identify user");
                    return Unauthorized(new { message = "Could not identify the user assigning the role." });
                }

                _logger.LogInformation(
                    "User {AssignedById} attempting to assign role {RoleId} to user {TargetUserId} in clinic {ClinicId}",
                    assignedById, roleId, assignRoleDto.UserId, assignRoleDto.ClinicId);

                // Validate clinic context if clinicId is specified
                if (assignRoleDto.ClinicId.HasValue)
                {
                    var hasGlobalRole = await _roleRepository.UserHasRoleAsync(assignedById, "Admin", null);

                    if (!hasGlobalRole)
                    {
                        var belongsToClinic = await _clinicRepository.UserBelongsToClinicAsync(
                            assignedById, assignRoleDto.ClinicId.Value);

                        if (!belongsToClinic)
                        {
                            _logger.LogWarning(
                                "Access denied: User {UserId} attempted to assign role in clinic {ClinicId} without membership",
                                assignedById, assignRoleDto.ClinicId);

                            return StatusCode(403, new
                            {
                                message = $"No tiene permisos para asignar roles en la clínica {assignRoleDto.ClinicId}."
                            });
                        }
                    }
                }

                // Prevent self-assignment
                if (assignRoleDto.UserId == assignedById)
                {
                    _logger.LogWarning(
                        "Security violation: User {UserId} attempted self-assignment of role {RoleId}",
                        assignedById, roleId);

                    return StatusCode(403, new
                    {
                        message = "No puede asignarse roles a sí mismo."
                    });
                }

                // Validate system role protection
                var role = await _roleRepository.GetRoleByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("Role assignment failed: Role {RoleId} not found", roleId);
                    return NotFound(new { message = $"Rol con ID {roleId} no encontrado." });
                }

                if (role.IsSystemRole)
                {
                    var isGlobalAdmin = await _roleRepository.UserHasRoleAsync(assignedById, "Admin", null);

                    if (!isGlobalAdmin)
                    {
                        _logger.LogWarning(
                            "Security violation: User {UserId} attempted to assign system role '{RoleName}' without admin privileges",
                            assignedById, role.Name);

                        return StatusCode(403, new
                        {
                            message = $"No tiene permisos para asignar el rol de sistema '{role.Name}'."
                        });
                    }
                }

                await _roleRepository.AssignRoleToUserAsync(
                    assignRoleDto.UserId, roleId, assignRoleDto.ClinicId,
                    assignRoleDto.ExpiresAt, assignedById);

                await _auditService.LogRoleAssignmentAsync(new RoleAuditLog
                {
                    UserId = assignRoleDto.UserId,
                    RoleId = roleId,
                    ClinicId = assignRoleDto.ClinicId,
                    AssignedByUserId = assignedById,
                    IPAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                    Reason = assignRoleDto.Reason,
                    ExpiresAt = assignRoleDto.ExpiresAt
                });

                _logger.LogInformation(
                    "Role {RoleId} successfully assigned to user {UserId} by {AssignedById}",
                    roleId, assignRoleDto.UserId, assignedById);

                return Ok(new { message = $"Role successfully assigned to user {assignRoleDto.UserId}." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Role assignment failed: Resource not found");
                return NotFound(new { message = "Recurso no encontrado." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error during role assignment: User {AssignedById} to {TargetUserId}, Role {RoleId}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value, assignRoleDto.UserId, roleId);

                return StatusCode(500, new { message = "Error interno del servidor. Por favor, contacte al administrador." });
            }
        }

        /// <summary>
        /// Remove a role from a user (optionally scoped to a clinic)
        /// </summary>
        [HttpPost("{roleId}/remove-from-user/{userId}")]
        [Authorize(Policy = "Roles.Revoke")]
        public async Task<IActionResult> RemoveRoleFromUser(
            int roleId,
            int userId,
            [FromQuery] int? clinicId = null,
            [FromBody] RoleRemovalDTO? removalDto = null)
        {
            try
            {
                var removedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(removedByUserId, out int removedById))
                {
                    _logger.LogWarning("Unauthorized role removal attempt - could not identify user");
                    return Unauthorized(new { message = "Could not identify the user removing the role." });
                }

                // Validate that justification is provided
                if (removalDto == null || string.IsNullOrWhiteSpace(removalDto.Reason))
                {
                    _logger.LogWarning("Role removal rejected: Missing justification for user {UserId}, role {RoleId}", userId, roleId);
                    return BadRequest(new { message = "Debe proporcionar una justificación para remover el rol" });
                }

                _logger.LogInformation(
                    "User {RemovedById} attempting to remove role {RoleId} from user {TargetUserId} in clinic {ClinicId}",
                    removedById, roleId, userId, clinicId);

                // Validate clinic context if clinicId is specified
                if (clinicId.HasValue)
                {
                    // Check if the user performing the action has a global role
                    var hasGlobalRole = await _roleRepository.UserHasRoleAsync(removedById, "Admin", null);

                    if (!hasGlobalRole)
                    {
                        // Check if user belongs to the clinic
                        var belongsToClinic = await _clinicRepository.UserBelongsToClinicAsync(
                            removedById,
                            clinicId.Value);

                        if (!belongsToClinic)
                        {
                            _logger.LogWarning(
                                "Access denied: User {UserId} attempted to remove role in clinic {ClinicId} without membership",
                                removedById, clinicId);

                            return StatusCode(403, new
                            {
                                message = $"No tiene permisos para remover roles en la clínica {clinicId}. Solo puede gestionar roles en clínicas a las que pertenece."
                            });
                        }
                    }
                }

                // Prevent self-removal (users cannot remove their own roles)
                if (userId == removedById)
                {
                    _logger.LogWarning(
                        "Security violation: User {UserId} attempted self-removal of role {RoleId}",
                        removedById, roleId);

                    return StatusCode(403, new
                    {
                        message = "No puede remover sus propios roles. La remoción de roles debe ser realizada por otro usuario autorizado."
                    });
                }

                // Validate system role protection
                var role = await _roleRepository.GetRoleByIdAsync(roleId);
                if (role == null)
                {
                    _logger.LogWarning("Role removal failed: Role {RoleId} not found", roleId);
                    return NotFound(new { message = $"Rol con ID {roleId} no encontrado." });
                }

                if (role.IsSystemRole)
                {
                    // Only global admins can revoke system roles
                    var isGlobalAdmin = await _roleRepository.UserHasRoleAsync(removedById, "Admin", null);

                    if (!isGlobalAdmin)
                    {
                        _logger.LogWarning(
                            "Security violation: User {UserId} attempted to remove system role '{RoleName}' without admin privileges",
                            removedById, role.Name);

                        return StatusCode(403, new
                        {
                            message = $"No tiene permisos para remover el rol de sistema '{role.Name}'. Solo administradores globales pueden remover roles de sistema."
                        });
                    }
                }

                await _roleRepository.RemoveRoleFromUserAsync(userId, roleId, clinicId);

                // Log audit trail for NOM-024 compliance
                await _auditService.LogRoleRemovalAsync(new RoleAuditLog
                {
                    UserId = userId,
                    RoleId = roleId,
                    ClinicId = clinicId,
                    AssignedByUserId = removedById,
                    IPAddress = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString(),
                    Reason = removalDto.Reason
                });

                _logger.LogInformation(
                    "Role {RoleId} successfully removed from user {UserId} by {RemovedById}",
                    roleId, userId, removedById);

                return Ok(new { message = $"Role successfully removed from user {userId}." });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "Role removal failed: Resource not found");
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Unexpected error during role removal: User {RemovedById} from {TargetUserId}, Role {RoleId}",
                    User.FindFirst(ClaimTypes.NameIdentifier)?.Value, userId, roleId);

                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get all roles assigned to a user
        /// </summary>
        [HttpGet("user/{userId}")]
        [Authorize(Policy = "Roles.View")]
        public async Task<ActionResult<IEnumerable<UserRoleDTO>>> GetUserRoles(int userId, [FromQuery] int? clinicId = null)
        {
            try
            {
                var userRoles = await _roleRepository.GetUserRolesAsync(userId, clinicId);
                var userRoleDTOs = _mapper.Map<IEnumerable<UserRoleDTO>>(userRoles);
                return Ok(userRoleDTOs);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Check if a user has a specific role
        /// </summary>
        [HttpGet("user/{userId}/has-role/{roleName}")]
        [Authorize(Policy = "Roles.View")]
        public async Task<ActionResult<object>> UserHasRole(int userId, string roleName, [FromQuery] int? clinicId = null)
        {
            try
            {
                var hasRole = await _roleRepository.UserHasRoleAsync(userId, roleName, clinicId);
                return Ok(new { userId, roleName, hasRole, clinicId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get role audit logs with optional filtering
        /// Supports filtering by userId, clinicId, and date range for NOM-024 compliance
        /// </summary>
        [HttpGet("audit-logs")]
        [Authorize(Policy = "Roles.ViewAudit")]
        public async Task<ActionResult<PagedResult<RoleAuditLog>>> GetAuditLogs(
            [FromQuery] int? userId = null,
            [FromQuery] int? clinicId = null,
            [FromQuery] string? action = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var requestingUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                _logger.LogInformation(
                    "User {UserId} requesting audit logs - Filters: UserId={FilterUserId}, ClinicId={FilterClinicId}, Action={Action}, DateRange={FromDate} to {ToDate}, Page={PageNumber}, PageSize={PageSize}",
                    requestingUserId, userId, clinicId, action, fromDate, toDate, pageNumber, pageSize);

                var result = await _auditService.GetAuditLogsAsync(
                    userId, clinicId, action, fromDate, toDate, pageNumber, pageSize);

                _logger.LogInformation(
                    "Audit logs retrieved: {TotalCount} total records, returning page {PageNumber} of {TotalPages}",
                    result.TotalCount, result.PageNumber, result.TotalPages);

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving audit logs");
                return StatusCode(500, new { message = "Error al obtener logs de auditoría." });
            }
        }
    }
}
