using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO de escritura para crear/actualizar mensajes de notificación
    /// </summary>
    public class NotificationMessageWriteDTO
    {
        /// <summary>ID del usuario destinatario - requerido</summary>
        [Required(ErrorMessage = "El ID del usuario es requerido")]
        public int UserId { get; set; }

        /// <summary>Destinatario (email, teléfono, etc.) - requerido</summary>
        [Required(ErrorMessage = "El destinatario es requerido")]
        [StringLength(255, MinimumLength = 1, ErrorMessage = "El destinatario debe tener entre 1 y 255 caracteres")]
        public string Recipient { get; set; }

        /// <summary>Asunto de la notificación (opcional, ej: para email)</summary>
        [StringLength(500, ErrorMessage = "El asunto no puede exceder 500 caracteres")]
        public string Subject { get; set; }

        /// <summary>Cuerpo del mensaje - requerido</summary>
        [Required(ErrorMessage = "El cuerpo del mensaje es requerido")]
        public string Body { get; set; }

        /// <summary>Tipo de notificación: Email, SMS, WhatsApp, Push - requerido</summary>
        [Required(ErrorMessage = "El tipo de notificación es requerido")]
        [StringLength(50, ErrorMessage = "El tipo no puede exceder 50 caracteres")]
        public string Type { get; set; }
    }
}
