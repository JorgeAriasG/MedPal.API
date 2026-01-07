using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO for role removal operations
    /// Requires justification for audit trail compliance
    /// </summary>
    public class RoleRemovalDTO
    {
        /// <summary>
        /// Mandatory justification for removing the role (NOM-024 compliance)
        /// </summary>
        [Required(ErrorMessage = "Debe proporcionar una justificación para la remoción del rol")]
        [MaxLength(500, ErrorMessage = "La justificación no puede exceder 500 caracteres")]
        public string Reason { get; set; } = string.Empty;
    }
}
