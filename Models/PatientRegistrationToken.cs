using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.Models
{
    /// <summary>
    /// Token único de completado de registro para pacientes "ghost" que agendan sin cuenta.
    /// Se almacena solo el hash SHA-256; single-use, con expiración y reenvío limitado (T02c).
    /// </summary>
    public class PatientRegistrationToken
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Paciente ghost que debe completar su registro (crear email + password).
        /// </summary>
        [Required]
        public int PatientId { get; set; }

        /// <summary>
        /// Hash SHA-256 (hex) del token enviado por WhatsApp. Ingresable por url/query; nunca se guarda el token crudo.
        /// </summary>
        [Required]
        [StringLength(64)]
        public string TokenHash { get; set; } = string.Empty;

        /// <summary>
        /// Estado: pending | used | revoked.
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "pending";

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Vigencia del token (72 horas por defecto).
        /// </summary>
        [Required]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddHours(72);

        public DateTime? UsedAt { get; set; }

        /// <summary>
        /// Número de reenvíos realizados; se limita a 3.
        /// </summary>
        public int ResendCount { get; set; } = 0;

        public DateTime? LastSentAt { get; set; }
    }
}