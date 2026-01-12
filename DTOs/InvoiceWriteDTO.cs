using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO de escritura para crear/actualizar facturas
    /// </summary>
    public class InvoiceWriteDTO
    {
        /// <summary>ID del paciente (requerido)</summary>
        [Required(ErrorMessage = "El paciente es requerido")]
        public int PatientId { get; set; }

        /// <summary>ID de la cita médica (requerido)</summary>
        [Required(ErrorMessage = "La cita es requerida")]
        public int AppointmentId { get; set; }

        /// <summary>Monto total de la factura (requerido, debe ser mayor a 0)</summary>
        [Required(ErrorMessage = "El monto total es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0.01")]
        public decimal TotalAmount { get; set; }

        /// <summary>Fecha de vencimiento (opcional)</summary>
        public DateTime? DueDate { get; set; }
    }
}
