using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Services
{
    /// <summary>
    /// Servicio para validaciones y operaciones de Payment
    /// Garantiza que los pagos sean consistentes con las facturas
    /// </summary>
    public interface IPaymentService
    {
        /// <summary>
        /// Valida que un Payment sea consistente
        /// - AmountPaid > 0
        /// - No excede el balance pendiente del Invoice
        /// - Invoice existe y no está eliminado
        /// </summary>
        Task<(bool IsValid, List<string> Errors)> ValidatePaymentAsync(Payment payment);

        /// <summary>
        /// Registra un nuevo pago y actualiza automáticamente el Invoice.PaidAmount y Status
        /// </summary>
        Task<Payment> CreatePaymentAsync(Payment payment, int? createdByUserId = null);

        /// <summary>
        /// Cancela un pago (soft delete) y actualiza el Invoice.PaidAmount
        /// </summary>
        Task<Payment> CancelPaymentAsync(int paymentId, int? deletedByUserId = null);

        /// <summary>
        /// Obtiene todos los pagos de una factura (excluyendo eliminados)
        /// </summary>
        Task<List<Payment>> GetPaymentsByInvoiceAsync(int invoiceId);

        /// <summary>
        /// Valida que la cantidad del pago sea válida para el Invoice
        /// </summary>
        Task<(bool IsValid, string ErrorMessage)> ValidatePaymentAmountAsync(int invoiceId, decimal amountPaid);
    }
}
