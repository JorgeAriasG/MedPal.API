using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MedPal.API.DTOs;
using MedPal.API.Services;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : BaseController
    {
        private readonly IDashboardService _dashboardService;
        private readonly IAuthorizationService _authorizationService;
        private readonly ITenantContextService _tenant;

        public DashboardController(
            IDashboardService dashboardService,
            IAuthorizationService authorizationService,
            ITenantContextService tenant)
        {
            _dashboardService = dashboardService;
            _authorizationService = authorizationService;
            _tenant = tenant;
        }

        /// <summary>
        /// GET api/dashboard/summary?clinicId={clinicId}&amp;date={yyyy-MM-dd}&amp;periodDays={7..365}
        /// Resumen agregado para la pantalla de Inicio.
        /// Filtros globales de tenancy + permisos por rol (Billing.View, ViewAuditLogPolicy, Appointments.ViewAll).
        /// </summary>
        [HttpGet("summary")]
        [Authorize(Policy = "ViewAppointmentsPolicy")]
        public async Task<ActionResult<DashboardSummaryReadDTO>> GetSummary(
            [FromQuery] int? clinicId = null,
            [FromQuery] DateOnly? date = null,
            [FromQuery] int? periodDays = null)
        {
            int? resolvedClinic = clinicId ?? _tenant.GetClinicId();
            if (resolvedClinic.HasValue && !await _tenant.HasAccessToClinicAsync(resolvedClinic.Value))
                return Forbid();

            var viewAll = (await _authorizationService.AuthorizeAsync(User, "Appointments.ViewAll")).Succeeded;
            var includeFinancial = (await _authorizationService.AuthorizeAsync(User, "Billing.View")).Succeeded;
            var includeActivity = _tenant.IsSuperAdmin || _tenant.IsAccountAdmin
                || (await _authorizationService.AuthorizeAsync(User, "ViewAuditLogPolicy")).Succeeded;

            var query = new DashboardQueryContext
            {
                Date = date ?? DateOnly.FromDateTime(DateTime.Today),
                ClinicId = resolvedClinic,
                CurrentUserId = _tenant.GetUserId() ?? 0,
                ViewAll = viewAll,
                IncludeFinancial = includeFinancial,
                IncludeActivity = includeActivity,
                PeriodDays = Math.Clamp(periodDays ?? 30, 7, 365),
            };

            var result = await _dashboardService.GetDashboardSummaryAsync(query);
            return Ok(result);
        }
    }
}