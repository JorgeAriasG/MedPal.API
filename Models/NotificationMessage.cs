using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    /// <summary>
    /// Modelo para mensajes de notificación
    /// Almacena notificaciones enviadas a usuarios (email, SMS, WhatsApp, push)
    /// </summary>
    public class NotificationMessage : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ID del usuario que recibe la notificación
        /// </summary>
        [ForeignKey("User")]
        public int? UserId { get; set; }

        /// <summary>
        /// Destinatario (email, número de teléfono, etc.)
        /// </summary>
        [Required]
        [StringLength(255)]
        public string Recipient { get; set; }

        /// <summary>
        /// Asunto de la notificación (si aplica, ej: email)
        /// </summary>
        [StringLength(500)]
        public string Subject { get; set; }

        /// <summary>
        /// Cuerpo del mensaje
        /// </summary>
        [Required]
        public string Body { get; set; }

        /// <summary>
        /// Tipo de notificación: Email, SMS, WhatsApp, Push
        /// </summary>
        [Required]
        public NotificationType Type { get; set; }

        /// <summary>
        /// Metadata adicional para la notificación (no se mapea a BD)
        /// Ejemplo: {"templateId": "welcome_email", "language": "es"}
        /// </summary>
        [NotMapped]
        public Dictionary<string, string> Metadata { get; set; }

        /// <summary>
        /// Si la notificación fue enviada exitosamente
        /// </summary>
        public bool IsSent { get; set; } = false;

        /// <summary>
        /// Fecha en que fue enviada
        /// </summary>
        public DateTime? SentAt { get; set; }

        /// <summary>
        /// Si la notificación fue leída/visto por el usuario
        /// </summary>
        public bool IsRead { get; set; } = false;

        /// <summary>
        /// Fecha en que fue leída
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// Fecha de creación
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ISoftDelete implementation
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        /// <summary>
        /// Navegación al usuario que recibe la notificación
        /// </summary>
        public virtual User User { get; set; }
    }

    /// <summary>
    /// Tipo de notificación disponible
    /// </summary>
    public enum NotificationType
    {
        /// <summary>Notificación por email</summary>
        Email = 0,

        /// <summary>Notificación por SMS</summary>
        SMS = 1,

        /// <summary>Notificación por WhatsApp</summary>
        WhatsApp = 2,

        /// <summary>Notificación push (mobile app)</summary>
        Push = 3
    }
}

