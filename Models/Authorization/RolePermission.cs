using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedPal.API.Models.Authorization
{
    /// <summary>
    /// Many-to-many junction table between Roles and Permissions
    /// Defines what permissions each role has
    /// </summary>
    public class RolePermission
    {
        [Required]
        public int RoleId { get; set; }

        [Required]
        public int PermissionId { get; set; }

        [Required]
        public DateTime GrantedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Who granted this permission to the role (for audit trail)
        /// </summary>
        public int? GrantedByUserId { get; set; }

        // Navigation properties
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; }

        [ForeignKey("PermissionId")]
        public virtual Permission Permission { get; set; }

        [ForeignKey("GrantedByUserId")]
        public virtual User GrantedByUser { get; set; }
    }
}
