using System;
using System.Threading.Tasks;
using MedPal.API.DTOs;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace MedPal.API.Controllers
{
    [ApiController]
    [Route("api/audit-logs")]
    [Authorize(Policy = "ViewAuditLogPolicy")]
    public class MedicalRecordAccessLogController : ControllerBase
    {
        private readonly IMedicalRecordAccessLogService _accessLogService;
        private readonly ILogger<MedicalRecordAccessLogController> _logger;

        public MedicalRecordAccessLogController(
            IMedicalRecordAccessLogService accessLogService,
            ILogger<MedicalRecordAccessLogController> logger)
        {
            _accessLogService = accessLogService;
            _logger = logger;
        }

        /// <summary>
        /// GET /api/audit-logs — paginated access logs with filters
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<AuditPagedResponseDto<MedicalRecordAccessLogReadDTO>>> GetAccessLogs(
            [FromQuery] int? userId,
            [FromQuery] int? clinicId,
            [FromQuery] int? patientId,
            [FromQuery] int? medicalHistoryId,
            [FromQuery] bool? hasConsent,
            [FromQuery] string? searchTerm,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 25)
        {
            var result = await _accessLogService.GetAccessLogsAsync(
                userId, clinicId, patientId, medicalHistoryId, hasConsent,
                searchTerm, dateFrom, dateTo, page, pageSize);

            var dtos = result.Items.Select(MapToReadDto);

            return Ok(new AuditPagedResponseDto<MedicalRecordAccessLogReadDTO>
            {
                Data = dtos,
                Pagination = new AuditPaginationDto
                {
                    Page = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalItems = result.TotalCount,
                    TotalPages = result.TotalPages
                }
            });
        }

        /// <summary>
        /// GET /api/audit-logs/{id} — single log detail
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<MedicalRecordAccessLogReadDTO>> GetAccessLogById(int id)
        {
            var log = await _accessLogService.GetAccessLogByIdAsync(id);
            if (log == null)
                return NotFound();

            return Ok(MapToReadDto(log));
        }

        /// <summary>
        /// GET /api/audit-logs/reports/generate — aggregated audit report
        /// </summary>
        [HttpGet("reports/generate")]
        public async Task<ActionResult<AuditReportDto>> GenerateReport(
            [FromQuery] int? clinicId,
            [FromQuery] int? patientId,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo)
        {
            var report = await _accessLogService.GenerateReportAsync(clinicId, patientId, dateFrom, dateTo);
            return Ok(report);
        }

        /// <summary>
        /// GET /api/audit-logs/export — export logs as CSV
        /// </summary>
        [HttpGet("export")]
        public async Task<IActionResult> ExportLogs(
            [FromQuery] int? userId,
            [FromQuery] int? clinicId,
            [FromQuery] int? patientId,
            [FromQuery] bool? hasConsent,
            [FromQuery] DateTime? dateFrom,
            [FromQuery] DateTime? dateTo,
            [FromQuery] string format = "csv")
        {
            var bytes = await _accessLogService.ExportLogsAsync(
                userId, clinicId, patientId, hasConsent, dateFrom, dateTo, format);

            var contentType = format.ToLower() == "csv" ? "text/csv" : "application/octet-stream";
            var fileName = $"audit-logs-{DateTime.UtcNow:yyyy-MM-dd}.{format}";

            return File(bytes, contentType, fileName);
        }

        private static MedicalRecordAccessLogReadDTO MapToReadDto(MedicalRecordAccessLog log)
        {
            return new MedicalRecordAccessLogReadDTO
            {
                Id = log.Id,
                UserId = log.UserId,
                UserName = log.User?.Name,
                MedicalHistoryId = log.MedicalHistoryId,
                PatientDetailsId = log.PatientDetailsId,
                PatientName = log.PatientDetails?.Patient?.Name,
                AccessTime = log.AccessTime,
                Purpose = log.Purpose,
                AccessingClinicId = log.AccessingClinicId,
                AccessingClinicName = log.AccessingClinic?.Name,
                MedicalRecordOwnerClinicId = log.MedicalRecordOwnerClinicId,
                OwnerClinicName = log.OwnerClinic?.Name,
                HadValidConsent = log.HadValidConsent,
                Reason = log.Reason,
                IpAddress = log.IpAddress,
                SessionId = log.SessionId
            };
        }
    }
}
