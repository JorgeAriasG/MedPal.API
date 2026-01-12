using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MedPal.API.Models
{
    /// <summary>
    /// Tabla de auditoría centralizada para cumplimiento normativo (NOM-004, LSSI-PC).
    /// Registra todos los cambios realizados en entidades sensibles.
    /// </summary>
    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }

        [Required]
        public string EntityType { get; set; }  // "Patient", "MedicalHistory", "Invoice", etc.

        [Required]
        public int EntityId { get; set; }

        [Required]
        public string Action { get; set; }  // "Create", "Update", "Delete", "View", "Export", "Download"

        /// <summary>
        /// Nombres de campos modificados, separados por coma
        /// Ej: "Email, Phone, Address"
        /// </summary>
        public string ChangedFields { get; set; }

        /// <summary>
        /// Valores anteriores en formato JSON
        /// </summary>
        public string OldValues { get; set; }

        /// <summary>
        /// Valores nuevos en formato JSON
        /// </summary>
        public string NewValues { get; set; }

        [Required]
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Dirección IP del cliente que realizó la acción
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// User-Agent del navegador/cliente
        /// </summary>
        public string UserAgent { get; set; }

        /// <summary>
        /// Descripción adicional de la acción
        /// </summary>
        public string Description { get; set; }

        public virtual User User { get; set; }
    }
}
