using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO para actualizar información de un usuario existente
    /// </summary>
    public class UserUpdateDTO
    {
        /// <summary>Nombre del usuario</summary>
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public required string Name { get; set; }

        /// <summary>Email del usuario (debe ser único)</summary>
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public required string Email { get; set; }

        /// <summary>Especialidad médica (opcional)</summary>
        [StringLength(100, ErrorMessage = "La especialidad no puede exceder 100 caracteres")]
        public string? Specialty { get; set; }

        /// <summary>Número de licencia profesional (opcional)</summary>
        [StringLength(100, ErrorMessage = "El número de licencia no puede exceder 100 caracteres")]
        public string? ProfessionalLicenseNumber { get; set; }

        /// <summary>Indica si el usuario está activo o inactivo</summary>
        public bool IsActive { get; set; }

        /// <summary>Indica si el usuario ha aceptado los términos de privacidad</summary>
        public bool HasAcceptedPrivacyTerms { get; set; }

        /// <summary>Id de la clínica principal (opcional, Fase 1)</summary>
        public int ClinicId { get; set; }
        
    }
}
