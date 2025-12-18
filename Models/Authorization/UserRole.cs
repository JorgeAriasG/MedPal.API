using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedPal.API.Models.Authorization
{
    /// <summary>
    /// Many-to-many junction table between Users and Roles
    /// Supports multi-tenancy: users can have different roles at different clinics
    /// Supports temporal roles: roles can expire at a specific date/time
    /// </summary>
    public class UserRole
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }

        /// <summary>
        /// Nullable: If null, role is global (applies to all clinics)
        /// If set, role only applies to this specific clinic
        /// Example: User is Admin at Clinic A, but just Doctor at Clinic B
        /// </summary>
        public int? ClinicId { get; set; }

        [Required]
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Optional: Role expires at this date/time
        /// Useful for temporary access grants (e.g., covering doctor for 2 weeks)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Who assigned this role (for audit trail)
        /// </summary>
        public int? AssignedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        [ForeignKey("ClinicId")]
        public virtual Clinic Clinic { get; set; }

        [ForeignKey("AssignedByUserId")]
        public virtual User AssignedByUser { get; set; }
    }
}
