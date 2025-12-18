using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO for assigning a role to a user
    /// </summary>
    public class AssignRoleDTO
    {
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Optional clinic ID for scoped roles (null for global role assignment)
        /// </summary>
        public int? ClinicId { get; set; }

        /// <summary>
        /// Optional expiration date for temporal role assignments
        /// </summary>
        public DateTime? ExpiresAt { get; set; }
    }
}
