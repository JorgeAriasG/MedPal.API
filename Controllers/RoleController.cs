using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using MedPal.API.DTOs;
using MedPal.API.Models.Authorization;
using MedPal.API.Repositories.Authorization;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MedPal.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RoleController : BaseController
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public RoleController(
            IRoleRepository roleRepository,
            IMapper mapper)
        {
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Get all active roles with their permissions
        /// </summary>
        [HttpGet]
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
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get a role by name
        /// </summary>
        [HttpGet("by-name/{name}")]
        public async Task<ActionResult<RoleReadDTO>> GetRoleByName(string name)
        {
            try
            {
                var role = await _roleRepository.GetRoleByNameAsync(name);
                if (role == null)
                    return NotFound(new { message = $"Role with name '{name}' not found." });

                var roleReadDTO = _mapper.Map<RoleReadDTO>(role);
                return Ok(roleReadDTO);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Assign a role to a user (optionally scoped to a clinic)
        /// </summary>
        [HttpPost("{roleId}/assign-to-user")]
        public async Task<IActionResult> AssignRoleToUser(int roleId, [FromBody] AssignRoleDTO assignRoleDto)
        {
            try
            {
                // Get the ID of the user performing this action
                var assignedByUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(assignedByUserId, out int assignedById))
                    return Unauthorized(new { message = "Could not identify the user assigning the role." });

                await _roleRepository.AssignRoleToUserAsync(
                    assignRoleDto.UserId,
                    roleId,
                    assignRoleDto.ClinicId,
                    assignRoleDto.ExpiresAt,
                    assignedById
                );

                return Ok(new { message = $"Role successfully assigned to user {assignRoleDto.UserId}." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Remove a role from a user (optionally scoped to a clinic)
        /// </summary>
        [HttpPost("{roleId}/remove-from-user/{userId}")]
        public async Task<IActionResult> RemoveRoleFromUser(int roleId, int userId, [FromQuery] int? clinicId = null)
        {
            try
            {
                await _roleRepository.RemoveRoleFromUserAsync(userId, roleId, clinicId);
                return Ok(new { message = $"Role successfully removed from user {userId}." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Internal server error: {ex.Message}" });
            }
        }

        /// <summary>
        /// Get all roles assigned to a user
        /// </summary>
        [HttpGet("user/{userId}")]
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
    }
}
