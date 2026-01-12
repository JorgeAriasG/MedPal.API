using System;
using System.ComponentModel.DataAnnotations;
using MedPal.API.Validation;

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
        /// Must be a future date if specified
        /// </summary>
        [FutureDate(ErrorMessage = "La fecha de expiración debe ser futura")]
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// Mandatory justification for the role assignment (NOM-024 compliance)
        /// </summary>
        [Required(ErrorMessage = "Debe proporcionar una justificación para la asignación del rol")]
        [MaxLength(500, ErrorMessage = "La justificación no puede exceder 500 caracteres")]
        public string Reason { get; set; } = string.Empty;
    }
}
