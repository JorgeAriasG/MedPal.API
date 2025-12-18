using System;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO for reading user role assignments
    /// </summary>
    public class UserRoleDTO
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        
        /// <summary>
        /// Clinic ID if this is a scoped role (null for global)
        /// </summary>
        public int? ClinicId { get; set; }
        
        /// <summary>
        /// Expiration date for temporal role assignments
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
        
        public DateTime AssignedAt { get; set; }
        public int? AssignedByUserId { get; set; }
        public bool IsActive { get; set; }
    }
}
