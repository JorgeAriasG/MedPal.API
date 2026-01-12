using System;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO de lectura para contactos de emergencia
    /// </summary>
    public class EmergencyContactReadDTO
    {
        /// <summary>ID del contacto de emergencia</summary>
        public int Id { get; set; }

        /// <summary>ID del paciente propietario</summary>
        public int PatientId { get; set; }

        /// <summary>Nombre completo del contacto</summary>
        public string FullName { get; set; }

        /// <summary>Relación con el paciente (Padre, Hermano, Cónyuge, etc.)</summary>
        public string Relationship { get; set; }

        /// <summary>Número de teléfono</summary>
        public string Phone { get; set; }

        /// <summary>Correo electrónico (opcional)</summary>
        public string Email { get; set; }

        /// <summary>Dirección (opcional)</summary>
        public string Address { get; set; }

        /// <summary>Prioridad (1-10, donde 1 es primario)</summary>
        public int Priority { get; set; }

        /// <summary>Indica si el contacto está activo</summary>
        public bool IsActive { get; set; }

        /// <summary>Fecha de creación</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Fecha de última actualización</summary>
        public DateTime UpdatedAt { get; set; }
    }
}
