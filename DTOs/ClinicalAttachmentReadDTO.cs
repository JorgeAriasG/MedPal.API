using System;

namespace MedPal.API.DTOs
{
    public class ClinicalAttachmentReadDTO
    {
        public int Id { get; set; }
        public int MedicalHistoryId { get; set; }
        public string Type { get; set; }
        public string FileName { get; set; }
        public string MimeType { get; set; }
        public long Size { get; set; }
        public int? UploadedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
