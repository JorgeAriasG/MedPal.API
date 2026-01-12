using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedPal.API.Data;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Services
{
    /// <summary>
    /// Implementación del servicio de Payment
    /// Maneja creación, validación y actualización de pagos con sincronización automática de Invoices
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly AppDbContext _context;
        private readonly IInvoiceService _invoiceService;

        public PaymentService(AppDbContext context, IInvoiceService invoiceService)
        {
            _context = context;
            _invoiceService = invoiceService;
        }

        /// <summary>
        /// Valida que un Payment sea consistente
        /// </summary>
        public async Task<(bool IsValid, List<string> Errors)> ValidatePaymentAsync(Payment payment)
        {
            var errors = new List<string>();

            // Validación 1: AmountPaid debe ser positivo
            if (payment.AmountPaid <= 0)
            {
                errors.Add("AmountPaid debe ser mayor a 0");
            }

            // Validación 2: Invoice debe existir
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == payment.InvoiceId && !i.IsDeleted);

            if (invoice == null)
            {
                errors.Add($"Invoice {payment.InvoiceId} no encontrado o está eliminado");
            }
            else
            {
                // Validación 3: Amount no puede exceder el balance pendiente
                var remainingAmount = invoice.RemainingAmount;
                if (payment.AmountPaid > remainingAmount)
                {
                    errors.Add($"AmountPaid ({payment.AmountPaid}) excede el balance pendiente ({remainingAmount})");
                }

                // Validación 4: Invoice no debe estar completamente pagado
                if (remainingAmount <= 0)
                {
                    errors.Add("El Invoice ya está completamente pagado");
                }
            }

            // Validación 5: PaymentMethod debe ser válido
            if (payment.PaymentMethod < 0)
            {
                errors.Add("PaymentMethod inválido");
            }

            // Validación 6: PaymentDate no puede ser en el futuro
            if (payment.PaymentDate > DateTime.UtcNow)
            {
                errors.Add("PaymentDate no puede ser en el futuro");
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Registra un nuevo pago y actualiza automáticamente el Invoice
        /// </summary>
        public async Task<Payment> CreatePaymentAsync(Payment payment, int? createdByUserId = null)
        {
            // Validar antes de crear
            var (isValid, errors) = await ValidatePaymentAsync(payment);
            if (!isValid)
            {
                throw new InvalidOperationException($"Payment inválido: {string.Join(", ", errors)}");
            }

            // Establecer timestamps
            payment.CreatedAt = DateTime.UtcNow;

            // Agregar a contexto
            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Actualizar el Invoice (PaidAmount y Status)
            await _invoiceService.UpdatePaidAmountAsync(payment.InvoiceId);

            return payment;
        }

        /// <summary>
        /// Cancela un pago (soft delete) y actualiza el Invoice
        /// </summary>
        public async Task<Payment> CancelPaymentAsync(int paymentId, int? deletedByUserId = null)
        {
            var payment = await _context.Payments
                .FirstOrDefaultAsync(p => p.Id == paymentId && !p.IsDeleted);

            if (payment == null)
            {
                throw new ArgumentException($"Payment {paymentId} no encontrado o ya está cancelado");
            }

            // Soft delete
            payment.IsDeleted = true;
            payment.DeletedAt = DateTime.UtcNow;
            payment.DeletedByUserId = deletedByUserId;
            payment.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Actualizar Invoice basado en los pagos restantes
            await _invoiceService.UpdatePaidAmountAsync(payment.InvoiceId);

            return payment;
        }

        /// <summary>
        /// Obtiene todos los pagos de una factura (excluyendo eliminados)
        /// </summary>
        public async Task<List<Payment>> GetPaymentsByInvoiceAsync(int invoiceId)
        {
            return await _context.Payments
                .Where(p => p.InvoiceId == invoiceId && !p.IsDeleted)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        /// <summary>
        /// Valida la cantidad del pago contra el Invoice
        /// </summary>
        public async Task<(bool IsValid, string ErrorMessage)> ValidatePaymentAmountAsync(int invoiceId, decimal amountPaid)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

            if (invoice == null)
            {
                return (false, "Invoice no encontrado o está eliminado");
            }

            if (amountPaid <= 0)
            {
                return (false, "El monto debe ser mayor a 0");
            }

            var remainingAmount = invoice.RemainingAmount;

            if (remainingAmount <= 0)
            {
                return (false, "El Invoice ya está completamente pagado");
            }

            if (amountPaid > remainingAmount)
            {
                return (false, $"El monto ({amountPaid}) excede el balance pendiente ({remainingAmount})");
            }

            return (true, string.Empty);
        }
    }
}
