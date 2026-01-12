using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Services
{
    /// <summary>
    /// Servicio para validaciones y operaciones de Invoice
    /// Garantiza integridad de datos de facturas y sincronización con pagos
    /// </summary>
    public interface IInvoiceService
    {
        /// <summary>
        /// Valida que un Invoice sea consistente
        /// - TotalAmount > 0
        /// - PaidAmount <= TotalAmount
        /// - Status es consistente con los pagos
        /// </summary>
        Task<(bool IsValid, List<string> Errors)> ValidateInvoiceAsync(Invoice invoice);

        /// <summary>
        /// Actualiza el PaidAmount basado en los pagos registrados
        /// También actualiza el Status (Pending, PartiallyPaid, Paid)
        /// </summary>
        Task<Invoice> UpdatePaidAmountAsync(int invoiceId);

        /// <summary>
        /// Obtiene el estado apropiado basado en PaidAmount y TotalAmount
        /// </summary>
        Enums.InvoiceStatus CalculateStatus(decimal totalAmount, decimal paidAmount);

        /// <summary>
        /// Valida que se pueda hacer un nuevo pago para este invoice
        /// Retorna error si ya está pagado o si la cantidad excede el balance
        /// </summary>
        Task<(bool CanPay, string ErrorMessage)> CanAcceptPaymentAsync(int invoiceId, decimal paymentAmount);
    }
}
