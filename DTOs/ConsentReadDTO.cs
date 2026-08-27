using System;

namespace MedPal.API.DTOs
{
    public class ConsentReadDTO
    {
        public int Id { get; set; }
        public int PatientDetailsId { get; set; }
        public int RequestingClinicId { get; set; }
        public int OwnerClinicId { get; set; }
        public int? TargetDoctorId { get; set; }
        public string ConsentScope { get; set; }
        public bool IsApproved { get; set; }
        public DateTime ConsentDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int? ApprovedByUserId { get; set; }
        public string? Notes { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Resolved navigation names — avoids N+1 API calls for the frontend
        public string? RequestingClinicName { get; set; }
        public string? OwnerClinicName { get; set; }
        public string? PatientName { get; set; }
        public string? ApprovedByUserName { get; set; }
        public string? TargetDoctorName { get; set; }
    }
}
