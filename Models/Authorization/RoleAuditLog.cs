using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedPal.API.Models.Authorization
{
    /// <summary>
    /// Audit log for role assignment and removal operations
    /// Ensures compliance with NOM-024-SSA3-2012 for traceability in health information systems
    /// </summary>
    public class RoleAuditLog
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// User who received or lost the role
        /// </summary>
        [Required]
        public int UserId { get; set; }

        /// <summary>
        /// Role that was assigned or removed
        /// </summary>
        [Required]
        public int RoleId { get; set; }

        /// <summary>
        /// Clinic context (null for global role operations)
        /// </summary>
        public int? ClinicId { get; set; }

        /// <summary>
        /// Action performed: "Assigned" or "Removed"
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// User who performed the role assignment/removal
        /// </summary>
        [Required]
        public int AssignedByUserId { get; set; }

        /// <summary>
        /// UTC timestamp of the operation
        /// </summary>
        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// IP address from which the operation was performed (for security auditing)
        /// </summary>
        [MaxLength(50)]
        public string? IPAddress { get; set; }

        /// <summary>
        /// Mandatory justification for the role change (NOM-024 compliance)
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string Reason { get; set; } = string.Empty;

        /// <summary>
        /// Expiration date if the role was temporary (only for "Assigned" actions)
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        // Navigation properties
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        [ForeignKey("RoleId")]
        public virtual Role? Role { get; set; }

        [ForeignKey("ClinicId")]
        public virtual Clinic? Clinic { get; set; }

        [ForeignKey("AssignedByUserId")]
        public virtual User? AssignedByUser { get; set; }
    }
}
