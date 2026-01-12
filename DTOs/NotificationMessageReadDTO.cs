using System;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO de lectura para mensajes de notificación
    /// </summary>
    public class NotificationMessageReadDTO
    {
        /// <summary>ID de la notificación</summary>
        public int Id { get; set; }

        /// <summary>ID del usuario que recibe la notificación (opcional)</summary>
        public int? UserId { get; set; }

        /// <summary>Destinatario (email, teléfono, etc.)</summary>
        public string Recipient { get; set; }

        /// <summary>Asunto de la notificación (ej: para email)</summary>
        public string Subject { get; set; }

        /// <summary>Cuerpo del mensaje</summary>
        public string Body { get; set; }

        /// <summary>Tipo de notificación (Email, SMS, WhatsApp, Push)</summary>
        public string Type { get; set; }

        /// <summary>Indica si la notificación fue enviada exitosamente</summary>
        public bool IsSent { get; set; }

        /// <summary>Fecha en que fue enviada</summary>
        public DateTime? SentAt { get; set; }

        /// <summary>Indica si la notificación fue leída/visto</summary>
        public bool IsRead { get; set; }

        /// <summary>Fecha en que fue leída</summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>Fecha de creación</summary>
        public DateTime CreatedAt { get; set; }
    }
}
