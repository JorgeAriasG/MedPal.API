using System;
using System.ComponentModel.DataAnnotations;

namespace MedPal.API.DTOs
{
    /// <summary>
    /// DTO de escritura para crear/actualizar pagos
    /// </summary>
    public class PaymentWriteDTO
    {
        /// <summary>ID de la factura a la que aplica el pago (requerido)</summary>
        [Required(ErrorMessage = "El invoice es requerido")]
        public int InvoiceId { get; set; }

        /// <summary>Monto a pagar (requerido, debe ser mayor a 0)</summary>
        [Required(ErrorMessage = "El monto es requerido")]
        [Range(0.01, double.MaxValue, ErrorMessage = "El monto debe ser mayor a 0.01")]
        public decimal AmountPaid { get; set; }

        /// <summary>Método de pago (Cash, CreditCard, BankTransfer, Insurance)</summary>
        [Required(ErrorMessage = "El método de pago es requerido")]
        [StringLength(50, ErrorMessage = "El método de pago no puede exceder 50 caracteres")]
        public string PaymentMethod { get; set; }

        /// <summary>Referencia de transacción (opcional, ej: número de comprobante)</summary>
        [StringLength(100, ErrorMessage = "La referencia de transacción no puede exceder 100 caracteres")]
        public string TransactionReference { get; set; }

        /// <summary>Fecha del pago (opcional, por defecto la fecha actual)</summary>
        public DateTime PaymentDate { get; set; } = DateTime.UtcNow;

        /// <summary>Notas adicionales (opcional)</summary>
        [StringLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string Notes { get; set; }
    }
}
