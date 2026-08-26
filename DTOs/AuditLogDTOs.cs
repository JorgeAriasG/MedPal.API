using System;

namespace MedPal.API.DTOs
{
    public class AuditLogReadDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string EntityType { get; set; }
        public int EntityId { get; set; }
        public string Action { get; set; }
        public string ChangedFields { get; set; }
        public string OldValues { get; set; }
        public string NewValues { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public string Description { get; set; }
    }

    public class AuditLogPagedResponseDto
    {
        public List<AuditLogReadDto> Data { get; set; }
        public AuditLogPaginationDto Pagination { get; set; }
    }

    public class AuditLogPaginationDto
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
