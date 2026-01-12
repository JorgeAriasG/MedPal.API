using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO para el registro de nuevos usuarios
    /// Los nuevos usuarios se registran automáticamente con el rol de Admin
    /// </summary>
    public class UserRegisterDTO
    {
        /// <summary>Nombre del usuario (requerido)</summary>
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 100 caracteres")]
        public string Name { get; set; }

        /// <summary>Email del usuario (requerido, debe ser único)</summary>
        [Required(ErrorMessage = "El email es requerido")]
        [EmailAddress(ErrorMessage = "El email no es válido")]
        public string Email { get; set; }

        /// <summary>Contraseña en texto plano (se hasheará en el servidor)</summary>
        [Required(ErrorMessage = "La contraseña es requerida")]
        [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres")]
        public string Password { get; set; }

        /// <summary>Confirmación de contraseña</summary>
        [Required(ErrorMessage = "La confirmación de contraseña es requerida")]
        [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
        public string ConfirmPassword { get; set; }

        /// <summary>Especialidad médica (opcional)</summary>
        [StringLength(100, ErrorMessage = "La especialidad no puede exceder 100 caracteres")]
        public string Specialty { get; set; }

        /// <summary>Número de licencia profesional (opcional)</summary>
        [StringLength(100, ErrorMessage = "El número de licencia no puede exceder 100 caracteres")]
        public string ProfessionalLicenseNumber { get; set; }

        /// <summary>Aceptación de términos de privacidad (requerido)</summary>
        [Required(ErrorMessage = "Debe aceptar los términos de privacidad")]
        public bool AcceptPrivacyTerms { get; set; }
    }
}
