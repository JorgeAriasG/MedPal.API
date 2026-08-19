using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    public class WhatsAppInteraction : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Appointment")]
        [Required]
        public int AppointmentId { get; set; }

        [ForeignKey("Patient")]
        [Required]
        public int PatientId { get; set; }

        [ForeignKey("NotificationMessage")]
        public int? NotificationMessageId { get; set; }

        /// <summary>
        /// ID del botón presionado (confirm, cancel)
        /// </summary>
        [Required]
        [StringLength(50)]
        public string ButtonId { get; set; } = string.Empty;

        /// <summary>
        /// Texto visible del botón (Confirmar, Cancelar)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string ButtonText { get; set; } = string.Empty;

        /// <summary>
        /// Número de teléfono del paciente (formato E.164)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string PatientPhone { get; set; } = string.Empty;

        /// <summary>
        /// WAMID del mensaje de respuesta del paciente
        /// </summary>
        [StringLength(255)]
        public string? Wamid { get; set; }

        /// <summary>
        /// Timestamp del mensaje recibido desde Meta
        /// </summary>
        public DateTime? MetaTimestamp { get; set; }

        /// <summary>
        /// Fecha en que se procesó la interacción en el backend
        /// </summary>
        [Required]
        public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Acción ejecutada en el sistema (Confirmed, Cancelled, RescheduleLinkSent)
        /// </summary>
        [StringLength(50)]
        public string? ActionTaken { get; set; }

        /// <summary>
        /// Mensaje de follow-up enviado al paciente después de la interacción
        /// </summary>
        public string? FollowUpMessage { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ISoftDelete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }
        public int? DeletedByUserId { get; set; }

        // Navigation
        public virtual Appointment Appointment { get; set; }
        public virtual Patient Patient { get; set; }
        public virtual NotificationMessage? NotificationMessage { get; set; }
    }
}
