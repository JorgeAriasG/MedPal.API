using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO de escritura para crear/actualizar contactos de emergencia
    /// </summary>
    public class EmergencyContactWriteDTO
    {
        /// <summary>Nombre completo del contacto (requerido)</summary>
        [Required(ErrorMessage = "El nombre es requerido")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "El nombre debe tener entre 1 y 255 caracteres")]
        public string FullName { get; set; }

        /// <summary>Relación con el paciente (requerido)</summary>
        [Required(ErrorMessage = "La relación es requerida")]
        [StringLength(100, MinimumLength = 1, ErrorMessage = "La relación debe tener entre 1 y 100 caracteres")]
        public string Relationship { get; set; }

        /// <summary>Número de teléfono (requerido)</summary>
        [Required(ErrorMessage = "El teléfono es requerido")]
        [Phone(ErrorMessage = "Formato de teléfono inválido")]
        public string Phone { get; set; }

        /// <summary>Correo electrónico (opcional)</summary>
        [EmailAddress(ErrorMessage = "Email inválido")]
        public string Email { get; set; }

        /// <summary>Dirección (opcional)</summary>
        [StringLength(500, ErrorMessage = "La dirección no puede exceder 500 caracteres")]
        public string Address { get; set; }

        /// <summary>Prioridad (1-10, donde 1 es primario, requerido)</summary>
        [Range(1, 10, ErrorMessage = "Prioridad debe estar entre 1 y 10")]
        public int Priority { get; set; } = 1;
    }
}
