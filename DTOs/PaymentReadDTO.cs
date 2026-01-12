using System;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO de lectura para pagos
    /// </summary>
    public class PaymentReadDTO
    {
        /// <summary>ID del pago</summary>
        public int Id { get; set; }

        /// <summary>ID de la factura asociada</summary>
        public int InvoiceId { get; set; }

        /// <summary>Monto pagado</summary>
        public decimal AmountPaid { get; set; }

        /// <summary>Método de pago (Cash, CreditCard, BankTransfer, Insurance)</summary>
        public string PaymentMethod { get; set; }

        /// <summary>Referencia de transacción (ej: número de comprobante)</summary>
        public string TransactionReference { get; set; }

        /// <summary>Fecha del pago</summary>
        public DateTime PaymentDate { get; set; }

        /// <summary>Notas adicionales</summary>
        public string Notes { get; set; }

        /// <summary>Indica si el pago ha sido eliminado (soft delete)</summary>
        public bool IsDeleted { get; set; }

        /// <summary>Fecha de creación</summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>Fecha de última actualización</summary>
        public DateTime UpdatedAt { get; set; }
    }
}
