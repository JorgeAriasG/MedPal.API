using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.Models.Authorization
{
    /// <summary>
    /// Represents a granular permission in the system
    /// Follows the Resource.Action naming pattern (e.g., "Patients.ViewAll")
    /// </summary>
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Full permission name: "Resource.Action" (e.g., "Appointments.Create")
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        /// <summary>
        /// Resource being protected (e.g., "Patients", "Appointments", "MedicalRecords")
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Resource { get; set; }

        /// <summary>
        /// Action being performed (e.g., "Create", "ViewAll", "Update", "Delete")
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Action { get; set; }

        /// <summary>
        /// Human-readable description of what this permission allows
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
    }
}
