using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MedPal.API.Data;
using MedPal.API.DTOs;
using MedPal.API.Enums;
using MedPal.API.Models;

namespace MedPal.API.Services.Implementations;

/// <summary>
/// Implementación del resumen del Dashboard (Inicio).
/// Todas las consultas usan AsNoTracking y los query filters globales de tenancy;
/// el contexto adicional (clínica explícita / solo-propias) lo aplica el servicio.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<DashboardSummaryReadDTO> GetDashboardSummaryAsync(DashboardQueryContext q)
    {
        var date = q.Date;
        var yesterday = date.AddDays(-1);
        var tomorrow = date.AddDays(1);
        var monthStart = new DateOnly(date.Year, date.Month, 1);
        var prevMonthStart = monthStart.AddMonths(-1);
        var prevMonthEnd = prevMonthStart.AddMonths(1).AddDays(-1);

        var monthStartD = new DateTime(monthStart.Year, monthStart.Month, 1, 0, 0, 0);
        var prevMonthStartD = monthStartD.AddMonths(-1);
        var prevMonthEndD = monthStartD.AddSeconds(-1);
        var todayStartD = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
        var todayEndD = new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);

        var appointments = _db.Appointments.AsNoTracking();
        if (q.ClinicId is int clinicId)
            appointments = appointments.Where(a => a.ClinicId == clinicId);
        if (!q.ViewAll)
            appointments = appointments.Where(a => a.UserId == q.CurrentUserId);

        var summary = new DashboardSummaryReadDTO { Date = date };

        // ---- KPI: Citas hoy ----
        var todayCounts = await appointments
            .Where(a => a.Date == date)
            .GroupBy(x => x.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();
        var yesterdayCount = await appointments.Where(a => a.Date == yesterday).CountAsync();
        var todayCount = todayCounts.Sum(x => x.Count);
        summary.AppointmentsToday = new DashboardAppointmentsKpiDTO
        {
            Count = todayCount,
            Scheduled = todayCounts.Where(x => x.Status == AppointmentStatus.Scheduled || x.Status == AppointmentStatus.Confirmed).Sum(x => x.Count),
            Completed = todayCounts.Where(x => x.Status == AppointmentStatus.Completed).Sum(x => x.Count),
            YesterdayCount = yesterdayCount,
            TrendPercent = yesterdayCount == 0 ? null : (double)(todayCount - yesterdayCount) / yesterdayCount * 100,
        };

        // ---- KPI: Pacientes ----
        var activeCount = await _db.Patients.AsNoTracking().CountAsync();
        var newThisMonth = await _db.Patients.AsNoTracking().Where(p => p.CreatedAt >= monthStartD).CountAsync();
        var prevMonthCount = await _db.Patients.AsNoTracking()
            .Where(p => p.CreatedAt >= prevMonthStartD && p.CreatedAt < monthStartD)
            .CountAsync();
        summary.Patients = new DashboardPatientsKpiDTO
        {
            ActiveCount = activeCount,
            NewThisMonth = newThisMonth,
            PreviousMonthCount = prevMonthCount,
            TrendPercent = prevMonthCount == 0 ? null : (double)(newThisMonth - prevMonthCount) / prevMonthCount * 100,
        };

        // ---- KPI: Consultas del mes ----
        var consThisMonth = await appointments.CountAsync(a =>
            a.Date >= monthStart && a.Date <= date && a.Status == AppointmentStatus.Completed);
        var consPrevMonth = await appointments.CountAsync(a =>
            a.Date >= prevMonthStart && a.Date <= prevMonthEnd && a.Status == AppointmentStatus.Completed);
        summary.ConsultationsThisMonth = new DashboardConsultationsKpiDTO
        {
            Count = consThisMonth,
            PreviousMonthCount = consPrevMonth,
            TrendPercent = consPrevMonth == 0 ? null : (double)(consThisMonth - consPrevMonth) / consPrevMonth * 100,
        };

        // ---- Ingresos (solo si el rol tiene permiso financiero) ----
        if (q.IncludeFinancial)
        {
            var payments = _db.Payments.AsNoTracking();
            var monthTotal = await payments.Where(p => p.PaymentDate >= monthStartD && p.PaymentDate <= todayEndD)
                .SumAsync(p => (decimal?)p.AmountPaid) ?? 0m;
            var prevTotal = await payments.Where(p => p.PaymentDate >= prevMonthStartD && p.PaymentDate < monthStartD)
                .SumAsync(p => (decimal?)p.AmountPaid) ?? 0m;

            var seriesStartD = monthStartD.AddMonths(-5);
            var raw = await payments
                .Where(p => p.PaymentDate >= seriesStartD && p.PaymentDate <= todayEndD)
                .Select(p => new { p.PaymentDate.Year, p.PaymentDate.Month, p.AmountPaid })
                .ToListAsync();
            var series = raw
                .GroupBy(x => new { x.Year, x.Month })
                .OrderBy(g => g.Key.Year).ThenBy(g => g.Key.Month)
                .Select(g => new DashboardRevenuePointDTO { Year = g.Key.Year, Month = g.Key.Month, Total = g.Sum(x => x.AmountPaid) })
                .ToList();

            summary.Revenue = new DashboardRevenueKpiDTO
            {
                Total = monthTotal,
                PreviousMonthTotal = prevTotal,
                TrendPercent = prevTotal == 0 ? null : (double)(monthTotal - prevTotal) / (double)prevTotal * 100,
                MonthlySeries = series,
            };
        }

        // ---- Agenda de hoy ----
        var firstApptIds = await appointments
            .GroupBy(a => a.PatientId)
            .Select(g => new { PatientId = g.Key, FirstId = g.Min(a => a.Id) })
            .ToDictionaryAsync(x => x.PatientId, x => x.FirstId);

        var todayRows = await appointments
            .Where(a => a.Date == date)
            .OrderBy(a => a.Time)
            .Select(a => new
            {
                a.Id,
                a.PatientId,
                a.Time,
                a.Status,
                Specialty = a.User.Specialty ?? "General",
                PatientName = a.Patient.Name + " " + a.Patient.Middlename + " " + a.Patient.Lastname,
            })
            .ToListAsync();

        summary.AgendaToday = todayRows.Select(r => new DashboardAppointmentDTO
        {
            Id = r.Id,
            PatientId = r.PatientId,
            PatientName = r.PatientName,
            Time = r.Time,
            Status = r.Status.ToString(),
            Specialty = r.Specialty,
            IsFirstVisit = firstApptIds.TryGetValue(r.PatientId, out var firstId) && firstId == r.Id,
        }).ToList();

        // ---- Próxima cita ----
        var nextAppt = await appointments
            .Where(a => a.Date >= date
                && a.Status != AppointmentStatus.Cancelled
                && a.Status != AppointmentStatus.NoShow
                && a.Status != AppointmentStatus.Completed)
            .OrderBy(a => a.Date)
            .ThenBy(a => a.Time)
            .Select(a => new
            {
                a.Id,
                a.PatientId,
                a.Time,
                a.Status,
                Specialty = a.User.Specialty ?? "General",
                PatientName = a.Patient.Name + " " + a.Patient.Middlename + " " + a.Patient.Lastname,
            })
            .FirstOrDefaultAsync();

        if (nextAppt != null)
        {
            summary.NextAppointment = new DashboardAppointmentDTO
            {
                Id = nextAppt.Id,
                PatientId = nextAppt.PatientId,
                PatientName = nextAppt.PatientName,
                Time = nextAppt.Time,
                Status = nextAppt.Status.ToString(),
                Specialty = nextAppt.Specialty,
                IsFirstVisit = firstApptIds.TryGetValue(nextAppt.PatientId, out var nf) && nf == nextAppt.Id,
            };
        }

        // ---- Pacientes recientes (últimos 5 registrados) ----
        var recentPatients = await _db.Patients.AsNoTracking()
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new { p.Id, FullName = p.Name + " " + p.Middlename + " " + p.Lastname })
            .ToListAsync();
        var recentIds = recentPatients.Select(p => p.Id).ToList();

        var withRecordIds = (await _db.PatientDetails.AsNoTracking()
            .Select(pd => pd.PatientId)
            .Distinct()
            .ToListAsync()).ToHashSet();

        var lastCompleted = await appointments
            .Where(a => a.Status == AppointmentStatus.Completed && recentIds.Contains(a.PatientId))
            .GroupBy(a => a.PatientId)
            .Select(g => new { PatientId = g.Key, Last = g.Max(a => a.Date) })
            .ToListAsync();
        var lastByPatient = lastCompleted.ToDictionary(x => x.PatientId, x => x.Last);

        var followUpCutoff = date.AddDays(-90);
        summary.RecentPatients = recentPatients.Select(p =>
        {
            var hasLast = lastByPatient.TryGetValue(p.Id, out var lastApt);
            return new DashboardPatientDTO
            {
                Id = p.Id,
                Name = p.FullName,
                Initials = BuildInitials(p.FullName),
                LastAppointmentDate = hasLast ? lastApt : (DateOnly?)null,
                NeedsFollowUp = hasLast && lastApt < followUpCutoff,
                HasRecord = withRecordIds.Contains(p.Id),
            };
        }).ToList();

        // ---- Donut por especialidad (consultas completadas en el periodo) ----
        var cutoff = date.AddDays(-q.PeriodDays);
        var specialtyRaw = await appointments
            .Where(a => a.Date >= cutoff && a.Date <= date && a.Status == AppointmentStatus.Completed)
            .Select(a => a.User.Specialty ?? "General")
            .ToListAsync();
        summary.Specialties = BuildSpecialties(specialtyRaw);

        // ---- Tareas operativas (derivadas de datos reales) ----
        var tasks = new List<DashboardTaskDTO>();

        var pendingConfirmations = await appointments.CountAsync(a =>
            a.Date >= date && a.Date <= tomorrow && a.Status == AppointmentStatus.Scheduled);
        if (pendingConfirmations > 0)
            tasks.Add(new DashboardTaskDTO { Type = "Confirmations", Count = pendingConfirmations, Priority = "High" });

        var activePrescriptions = await _db.Prescriptions.AsNoTracking()
            .CountAsync(p => p.Status == PrescriptionStatus.Active && p.ExpiresAt > todayStartD);
        if (activePrescriptions > 0)
            tasks.Add(new DashboardTaskDTO { Type = "Prescriptions", Count = activePrescriptions, Priority = "High" });

        var incompleteRecords = Math.Max(0, activeCount - withRecordIds.Count);
        if (incompleteRecords > 0)
            tasks.Add(new DashboardTaskDTO { Type = "IncompleteRecords", Count = incompleteRecords, Priority = "Medium" });

        var remindersToSend = await appointments.CountAsync(a =>
            a.Date == tomorrow
            && a.ReminderSentAt == null
            && (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed));
        if (remindersToSend > 0)
            tasks.Add(new DashboardTaskDTO { Type = "Reminders", Count = remindersToSend, Priority = "Medium" });

        summary.Tasks = tasks;

        // ---- Actividad reciente (solo si el rol tiene acceso a auditoría) ----
        if (q.IncludeActivity)
        {
            summary.RecentActivity = await _db.AuditLogs.AsNoTracking()
                .Where(l => _db.Users.AsNoTracking().Any(u => u.Id == l.UserId))
                .OrderByDescending(l => l.Timestamp)
                .Take(10)
                .Select(l => new DashboardActivityDTO
                {
                    Id = l.Id,
                    EntityType = l.EntityType,
                    Action = l.Action,
                    EntityId = l.EntityId,
                    Description = l.Description ?? "",
                    Timestamp = l.Timestamp,
                })
                .ToListAsync();
        }

        return summary;
    }

    private static List<DashboardSpecialtyStatDTO> BuildSpecialties(List<string> raw)
    {
        var grouped = raw.GroupBy(x => x)
            .Select(g => new DashboardSpecialtyStatDTO { Name = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        if (grouped.Count <= 6)
            return grouped;

        var top = grouped.Take(5).ToList();
        top.Add(new DashboardSpecialtyStatDTO { Name = "Otras", Count = grouped.Skip(5).Sum(x => x.Count) });
        return top;
    }

    private static string BuildInitials(string fullName)
    {
        var parts = (fullName ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
            return "?";
        char first = parts[0][0];
        char last = parts.Length > 1 ? parts[^1][0] : (parts[0].Length > 1 ? parts[0][1] : first);
        return $"{first}{last}".ToUpperInvariant();
    }
}