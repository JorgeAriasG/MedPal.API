using System;
using System.Collections.Generic;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO de lectura para facturas
    /// </summary>
    public class InvoiceReadDTO
    {
        /// <summary>ID de la factura</summary>
        public int Id { get; set; }

        /// <summary>ID del paciente</summary>
        public int PatientId { get; set; }

        /// <summary>ID de la cita médica asociada</summary>
        public int AppointmentId { get; set; }

        /// <summary>Monto total de la factura</summary>
        public decimal TotalAmount { get; set; }

        /// <summary>Monto pagado hasta el momento</summary>
        public decimal PaidAmount { get; set; }

        /// <summary>Monto pendiente (calculado como TotalAmount - PaidAmount)</summary>
        public decimal RemainingAmount { get; set; }

        /// <summary>Estado de la factura (Pending, PartiallyPaid, Paid, Overdue, Cancelled)</summary>
        public string Status { get; set; }

        /// <summary>Fecha de vencimiento (opcional)</summary>
        public DateTime? DueDate { get; set; }

        /// <summary>Fecha de creación</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Fecha de última actualización</summary>
        public DateTime UpdatedAt { get; set; }

        /// <summary>Pagos asociados a esta factura</summary>
        public List<PaymentReadDTO> Payments { get; set; } = new();
    }
}
