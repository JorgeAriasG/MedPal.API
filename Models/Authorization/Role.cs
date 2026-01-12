using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.Models.Authorization
{
    /// <summary>
    /// Represents a role in the system (Admin, Doctor, Nurse, etc.)
    /// Roles are dynamically managed in the database for flexibility
    /// </summary>
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public string Name { get; set; } // "Admin", "Doctor", "Nurse", "Receptionist", "Patient"

        [MaxLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// System roles cannot be deleted (Admin, Doctor, Patient, etc.)
        /// Custom roles created by users can be deleted
        /// </summary>
        public bool IsSystemRole { get; set; } = false;

        /// <summary>
        /// Allows temporary deactivation of roles without deletion
        /// </summary>
        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? DeletedAt { get; set; }

        // Navigation properties
        public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
