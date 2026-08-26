using System;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Services
{
    /// <summary>
    /// Service interface for medical record access audit logging (NOM-004 compliance)
    /// </summary>
    public interface IMedicalRecordAccessLogService
    {
        /// <summary>
        /// Log an access to a medical record
        /// </summary>
        Task LogAccessAsync(MedicalRecordAccessLog accessLog);

        /// <summary>
        /// Get paginated access logs with filters
        /// </summary>
        Task<MedPal.API.DTOs.PagedResult<MedicalRecordAccessLog>> GetAccessLogsAsync(
            int? userId = null,
            int? clinicId = null,
            int? patientId = null,
            int? medicalHistoryId = null,
            bool? hasConsent = null,
            string? searchTerm = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            int pageNumber = 1,
            int pageSize = 25);

        /// <summary>
        /// Get a single access log by ID
        /// </summary>
        Task<MedicalRecordAccessLog?> GetAccessLogByIdAsync(int id);

        /// <summary>
        /// Generate aggregated audit report
        /// </summary>
        Task<DTOs.AuditReportDto> GenerateReportAsync(
            int? clinicId = null,
            int? patientId = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null);

        /// <summary>
        /// Export access logs as CSV byte array
        /// </summary>
        Task<byte[]> ExportLogsAsync(
            int? userId = null,
            int? clinicId = null,
            int? patientId = null,
            bool? hasConsent = null,
            DateTime? dateFrom = null,
            DateTime? dateTo = null,
            string format = "csv");
    }
}
