using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MedPal.API.Data;
using MedPal.API.DTOs;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MedPal.API.Services
{
    /// <summary>
    /// Service for managing medical record access audit logs (NOM-004 compliance)
    /// </summary>
    public class MedicalRecordAccessLogService : IMedicalRecordAccessLogService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MedicalRecordAccessLogService> _logger;

        public MedicalRecordAccessLogService(AppDbContext context, ILogger<MedicalRecordAccessLogService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogAccessAsync(MedicalRecordAccessLog accessLog)
        {
            if (accessLog == null)
                throw new ArgumentNullException(nameof(accessLog));

            accessLog.AccessTime = DateTime.UtcNow;

            _context.MedicalRecordAccessLogs.Add(accessLog);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Medical record access: User {UserId} accessed patient {PatientId} record (HistoryId={HistoryId}) from clinic {ClinicId}. Purpose: {Purpose}",
                accessLog.UserId,
                accessLog.PatientDetailsId,
                accessLog.MedicalHistoryId,
                accessLog.AccessingClinicId,
                accessLog.Purpose);
        }

        public async Task<PagedResult<MedicalRecordAccessLog>> GetAccessLogsAsync(
            int? userId = null,
            int? clinicId = null,
            int? patientId = null,
            int? medicalHistoryId = null,
            bool? hasConsent = null,
            string? searchTerm = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int pageNumber = 1,
            int pageSize = 25)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 25;
            if (pageSize > 100) pageSize = 100;

            var query = _context.MedicalRecordAccessLogs
                .Include(a => a.User)
                .Include(a => a.PatientDetails)
                    .ThenInclude(p => p!.Patient)
                .Include(a => a.AccessingClinic)
                .Include(a => a.OwnerClinic)
                .AsQueryable();

            if (userId.HasValue)
                query = query.Where(a => a.UserId == userId);

            if (clinicId.HasValue)
                query = query.Where(a => a.AccessingClinicId == clinicId);

            if (patientId.HasValue)
                query = query.Where(a => a.PatientDetailsId == patientId);

            if (medicalHistoryId.HasValue)
                query = query.Where(a => a.MedicalHistoryId == medicalHistoryId);

            if (hasConsent.HasValue)
                query = query.Where(a => a.HadValidConsent == hasConsent);

            if (dateFrom.HasValue)
                query = query.Where(a => a.AccessTime >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(a => a.AccessTime <= dateTo.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                var term = searchTerm.ToLower();
                query = query.Where(a =>
                    a.Purpose.ToLower().Contains(term) ||
                    (a.Reason != null && a.Reason.ToLower().Contains(term)));
            }

            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            var items = await query
                .OrderByDescending(a => a.AccessTime)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<MedicalRecordAccessLog>
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };
        }

        public async Task<MedicalRecordAccessLog?> GetAccessLogByIdAsync(int id)
        {
            return await _context.MedicalRecordAccessLogs
                .Include(a => a.User)
                .Include(a => a.PatientDetails)
                    .ThenInclude(p => p!.Patient)
                .Include(a => a.MedicalHistory)
                .Include(a => a.AccessingClinic)
                .Include(a => a.OwnerClinic)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<AuditReportDto> GenerateReportAsync(
            int? clinicId = null,
            int? patientId = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null)
        {
            var query = _context.MedicalRecordAccessLogs
                .Include(a => a.User)
                .Include(a => a.AccessingClinic)
                .AsQueryable();

            if (clinicId.HasValue)
                query = query.Where(a => a.AccessingClinicId == clinicId);

            if (patientId.HasValue)
                query = query.Where(a => a.PatientDetailsId == patientId);

            if (dateFrom.HasValue)
                query = query.Where(a => a.AccessTime >= dateFrom.Value);

            if (dateTo.HasValue)
                query = query.Where(a => a.AccessTime <= dateTo.Value);

            var logs = await query.ToListAsync();

            var accessesByUser = logs
                .GroupBy(a => a.UserId)
                .Select(g => new AccessByUserReportDto
                {
                    UserId = g.Key,
                    UserName = g.First().User?.Name ?? "Unknown",
                    AccessCount = g.Count(),
                    LastAccessTime = g.Max(a => a.AccessTime)
                })
                .OrderByDescending(x => x.AccessCount)
                .ToList();

            var accessesByClinic = logs
                .GroupBy(a => a.AccessingClinicId)
                .Select(g => new AccessByClinicReportDto
                {
                    ClinicId = g.Key,
                    ClinicName = g.First().AccessingClinic?.Name ?? "Unknown",
                    AccessCount = g.Count(),
                    ConsentViolationCount = g.Count(a => !a.HadValidConsent)
                })
                .OrderByDescending(x => x.AccessCount)
                .ToList();

            var accessesByDate = logs
                .GroupBy(a => a.AccessTime.Date)
                .Select(g => new AccessByDateReportDto
                {
                    Date = g.Key,
                    AccessCount = g.Count(),
                    ConsentViolationCount = g.Count(a => !a.HadValidConsent)
                })
                .OrderBy(x => x.Date)
                .ToList();

            return new AuditReportDto
            {
                TotalAccesses = logs.Count,
                AccessesByUser = accessesByUser,
                AccessesByClinic = accessesByClinic,
                AccessesByDate = accessesByDate,
                ConsentViolations = logs.Count(a => !a.HadValidConsent),
                GeneratedAt = DateTime.UtcNow
            };
        }

        public async Task<byte[]> ExportLogsAsync(
            int? userId = null,
            int? clinicId = null,
            int? patientId = null,
            bool? hasConsent = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            string format = "csv")
        {
            var paged = await GetAccessLogsAsync(
                userId, clinicId, patientId, null, hasConsent, null,
                dateFrom, dateTo, 1, 10000);

            if (format.ToLower() == "csv")
                return BuildCsv(paged.Items);

            return Encoding.UTF8.GetBytes("Export format not supported");
        }

        private byte[] BuildCsv(IEnumerable<MedicalRecordAccessLog> logs)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Id,UserId,UserName,PatientDetailsId,AccessTime,Purpose,AccessingClinicId,MedicalRecordOwnerClinicId,HadValidConsent,Reason,IpAddress,SessionId");

            foreach (var log in logs)
            {
                sb.AppendLine($"{log.Id},{log.UserId},\"{Escape(log.User?.Name)}\",{log.PatientDetailsId},{log.AccessTime:O},\"{Escape(log.Purpose)}\",{log.AccessingClinicId},{log.MedicalRecordOwnerClinicId},{log.HadValidConsent},\"{Escape(log.Reason)}\",\"{Escape(log.IpAddress)}\",\"{Escape(log.SessionId)}\"");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }

        private static string Escape(string? value) => (value ?? "").Replace("\"", "\"\"");
    }
}
