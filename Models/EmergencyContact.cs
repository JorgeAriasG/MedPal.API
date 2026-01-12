using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using MedPal.API.Interfaces;

namespace MedPal.API.Models
{
    /// <summary>
    /// Modelo para Contacto de Emergencia
    /// Almacena información de contactos de emergencia para pacientes
    /// Permite múltiples contactos con prioridad definida
    /// </summary>
    public class EmergencyContact : ISoftDelete
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("Patient")]
        [Required]
        public int PatientId { get; set; }

        /// <summary>
        /// Nombre completo del contacto de emergencia
        /// </summary>
        [Required]
        [StringLength(255)]
        public string FullName { get; set; }

        /// <summary>
        /// Relación con el paciente (Padre, Madre, Hermano, Cónyuge, Amigo, Otro, etc.)
        /// </summary>
        [Required]
        [StringLength(100)]
        public string Relationship { get; set; }

        /// <summary>
        /// Teléfono del contacto de emergencia (requerido)
        /// </summary>
        [Required]
        [StringLength(20)]
        public string Phone { get; set; }

        /// <summary>
        /// Email del contacto de emergencia (opcional)
        /// </summary>
        [StringLength(255)]
        public string Email { get; set; }

        /// <summary>
        /// Dirección del contacto de emergencia (opcional)
        /// </summary>
        [StringLength(500)]
        public string Address { get; set; }

        /// <summary>
        /// Prioridad del contacto (1=primario, 2=secundario, 3=terciario, etc.)
        /// Menor número = mayor prioridad
        /// </summary>
        [Range(1, 10)]
        public int Priority { get; set; } = 1;

        /// <summary>
        /// Si este contacto es el contacto de emergencia activo/primario
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Fecha de creación del registro
        /// </summary>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización del registro
        /// </summary>
        [Required]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // ISoftDelete implementation
        /// <summary>
        /// Indica si el contacto ha sido eliminado lógicamente
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        /// <summary>
        /// Fecha de eliminación lógica
        /// </summary>
        public DateTime? DeletedAt { get; set; }

        /// <summary>
        /// ID del usuario que eliminó el registro
        /// </summary>
        public int? DeletedByUserId { get; set; }

        /// <summary>
        /// Navegación al paciente
        /// </summary>
        public virtual Patient Patient { get; set; }
    }
}
