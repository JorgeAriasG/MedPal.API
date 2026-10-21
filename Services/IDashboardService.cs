using System;
using System.Threading.Tasks;
using MedPal.API.DTOs;

namespace MedPal.API.Services;

/// <summary>
/// Contexto de la consulta del dashboard: tenancy, alcance por rol
/// y permisos calculados por el controlador.
/// </summary>
public sealed class DashboardQueryContext
{
    public DateOnly Date { get; set; }
    public int? ClinicId { get; set; }
    public int CurrentUserId { get; set; }
    public bool ViewAll { get; set; }
    public bool IncludeFinancial { get; set; }
    public bool IncludeActivity { get; set; }
    public int PeriodDays { get; set; } = 30;
}

/// <summary>
/// Servicio de resumen para la pantalla de Inicio (Dashboard).
/// Consultas agregadas, AsNoTracking, tenant-scoped vía query filters.
/// </summary>
public interface IDashboardService
{
    Task<DashboardSummaryReadDTO> GetDashboardSummaryAsync(DashboardQueryContext query);
}