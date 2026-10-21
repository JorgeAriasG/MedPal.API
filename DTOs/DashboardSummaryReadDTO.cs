using System;
using System.Collections.Generic;

namespace MedPal.API.DTOs;

/// <summary>
/// Resumen agregado para la pantalla de Inicio (Dashboard).
/// Un único roundtrip por widget con datos reales, tenant-scoped y role-aware.
/// </summary>
public class DashboardSummaryReadDTO
{
    public DateOnly Date { get; set; }

    /// <summary>KPIs de citas del día actual.</summary>
    public DashboardAppointmentsKpiDTO AppointmentsToday { get; set; }

    /// <summary>KPIs de pacientes activos.</summary>
    public DashboardPatientsKpiDTO Patients { get; set; }

    /// <summary>KPIs de consultas completadas del mes.</summary>
    public DashboardConsultationsKpiDTO ConsultationsThisMonth { get; set; }

    /// <summary>Ingresos del mes. Null si el rol no tiene permiso financiero.</summary>
    public DashboardRevenueKpiDTO Revenue { get; set; }

    /// <summary>Agenda cronológica del día.</summary>
    public List<DashboardAppointmentDTO> AgendaToday { get; set; } = new();

    /// <summary>Próxima cita (hoy o futura) elegible para iniciar.</summary>
    public DashboardAppointmentDTO NextAppointment { get; set; }

    /// <summary>Últimos 5 pacientes registrados.</summary>
    public List<DashboardPatientDTO> RecentPatients { get; set; } = new();

    /// <summary>Consultas completadas por especialidad (periodo configurable).</summary>
    public List<DashboardSpecialtyStatDTO> Specialties { get; set; } = new();

    /// <summary>Cola operativa de tareas derivadas de datos reales.</summary>
    public List<DashboardTaskDTO> Tasks { get; set; } = new();

    /// <summary>Actividad reciente (solo roles con acceso a auditoría).</summary>
    public List<DashboardActivityDTO> RecentActivity { get; set; } = new();
}

public class DashboardAppointmentsKpiDTO
{
    public int Count { get; set; }
    public int Scheduled { get; set; }
    public int Completed { get; set; }
    public int YesterdayCount { get; set; }
    public double? TrendPercent { get; set; }
}

public class DashboardPatientsKpiDTO
{
    public int ActiveCount { get; set; }
    public int NewThisMonth { get; set; }
    public int PreviousMonthCount { get; set; }
    public double? TrendPercent { get; set; }
}

public class DashboardConsultationsKpiDTO
{
    public int Count { get; set; }
    public int PreviousMonthCount { get; set; }
    public double? TrendPercent { get; set; }
}

public class DashboardRevenueKpiDTO
{
    public decimal Total { get; set; }
    public decimal PreviousMonthTotal { get; set; }
    public double? TrendPercent { get; set; }
    public List<DashboardRevenuePointDTO> MonthlySeries { get; set; } = new();
}

public class DashboardRevenuePointDTO
{
    public int Year { get; set; }
    public int Month { get; set; }
    public decimal Total { get; set; }
}

public class DashboardAppointmentDTO
{
    public int Id { get; set; }
    public int PatientId { get; set; }
    public string PatientName { get; set; }
    public TimeOnly Time { get; set; }
    public string Status { get; set; }
    public string Specialty { get; set; }
    public bool IsFirstVisit { get; set; }
}

public class DashboardPatientDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Initials { get; set; }
    public DateOnly? LastAppointmentDate { get; set; }
    public bool NeedsFollowUp { get; set; }
    public bool HasRecord { get; set; }
}

public class DashboardSpecialtyStatDTO
{
    public string Name { get; set; }
    public int Count { get; set; }
}

public class DashboardTaskDTO
{
    /// <summary>Confirmations | Prescriptions | IncompleteRecords | Reminders</summary>
    public string Type { get; set; }
    public int Count { get; set; }
    /// <summary>High | Medium | Low</summary>
    public string Priority { get; set; }
}

public class DashboardActivityDTO
{
    public int Id { get; set; }
    public string EntityType { get; set; }
    public string Action { get; set; }
    public int EntityId { get; set; }
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
}