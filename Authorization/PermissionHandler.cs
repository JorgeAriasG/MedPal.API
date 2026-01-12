using System.Security.Claims;
using MedPal.API.Repositories.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace MedPal.API.Authorization
{
    /// <summary>
    /// Authorization handler for permission-based policies
    /// Validates if a user has a specific permission through their roles
    /// </summary>
    public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
    {
        private readonly IPermissionRepository _permissionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PermissionHandler(
            IPermissionRepository permissionRepository,
            IHttpContextAccessor httpContextAccessor)
        {
            _permissionRepository = permissionRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            PermissionRequirement requirement)
        {
            // 1. Get userId from JWT token (already authenticated by [Authorize])
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                context.Fail();
                return;
            }

            // 2. Get clinicId from request context if applicable
            int? clinicId = GetClinicIdFromRequest();

            // 3. Check if user has the permission (clinicId can be null for global permissions)
            bool hasPermission = await _permissionRepository.UserHasPermissionAsync(
                userId,
                requirement.PermissionName,
                clinicId
            );

            if (hasPermission)
            {
                context.Succeed(requirement);
            }
            else
            {
                Console.WriteLine($"Permission denied for user {userId}: {requirement.PermissionName} (clinicId: {clinicId})");
                context.Fail();
            }
        }

        /// <summary>
        /// Extract clinicId from request headers, query params, or route data
        /// </summary>
        private int? GetClinicIdFromRequest()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null)
                return null;

            // Try to get from header
            if (httpContext.Request.Headers.TryGetValue("X-Clinic-Id", out var clinicIdHeader))
            {
                if (int.TryParse(clinicIdHeader, out int clinicId))
                    return clinicId;
            }

            // Try to get from query string
            if (httpContext.Request.Query.TryGetValue("clinicId", out var clinicIdQuery))
            {
                if (int.TryParse(clinicIdQuery, out int clinicId))
                    return clinicId;
            }

            // Try to get from route data
            if (httpContext.Request.RouteValues.TryGetValue("clinicId", out var clinicIdRoute))
            {
                if (int.TryParse(clinicIdRoute?.ToString(), out int clinicId))
                    return clinicId;
            }

            return null;
        }
    }
}
