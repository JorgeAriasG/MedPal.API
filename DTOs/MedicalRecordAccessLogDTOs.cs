using System;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// Read DTO for MedicalRecordAccessLog — matches frontend IMedicalRecordAccessLog
    /// </summary>
    public class MedicalRecordAccessLogReadDTO
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string? UserName { get; set; }
        public int? MedicalHistoryId { get; set; }
        public int PatientDetailsId { get; set; }
        public string? PatientName { get; set; }
        public DateTime AccessTime { get; set; }
        public string Purpose { get; set; } = "Treatment";
        public int AccessingClinicId { get; set; }
        public string? AccessingClinicName { get; set; }
        public int MedicalRecordOwnerClinicId { get; set; }
        public string? OwnerClinicName { get; set; }
        public bool HadValidConsent { get; set; }
        public string? Reason { get; set; }
        public string? IpAddress { get; set; }
        public string? SessionId { get; set; }
    }

    /// <summary>
    /// Pagination metadata — matches frontend pagination object
    /// </summary>
    public class AuditPaginationDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }

    /// <summary>
    /// Paginated response wrapper — matches frontend PagedResult shape
    /// </summary>
    public class AuditPagedResponseDto<T>
    {
        public System.Collections.Generic.IEnumerable<T> Data { get; set; } = new System.Collections.Generic.List<T>();
        public AuditPaginationDto Pagination { get; set; } = new();
    }

    /// <summary>
    /// Audit report DTO — matches frontend AuditReport
    /// </summary>
    public class AuditReportDto
    {
        public int TotalAccesses { get; set; }
        public System.Collections.Generic.List<AccessByUserReportDto> AccessesByUser { get; set; } = new();
        public System.Collections.Generic.List<AccessByClinicReportDto> AccessesByClinic { get; set; } = new();
        public System.Collections.Generic.List<AccessByDateReportDto> AccessesByDate { get; set; } = new();
        public int ConsentViolations { get; set; }
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }

    public class AccessByUserReportDto
    {
        public int UserId { get; set; }
        public string UserName { get; set; } = "";
        public int AccessCount { get; set; }
        public DateTime LastAccessTime { get; set; }
    }

    public class AccessByClinicReportDto
    {
        public int ClinicId { get; set; }
        public string ClinicName { get; set; } = "";
        public int AccessCount { get; set; }
        public int ConsentViolationCount { get; set; }
    }

    public class AccessByDateReportDto
    {
        public DateTime Date { get; set; }
        public int AccessCount { get; set; }
        public int ConsentViolationCount { get; set; }
    }
}
