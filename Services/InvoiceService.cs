using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MedPal.API.Data;
using MedPal.API.Enums;
using MedPal.API.Models;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Services
{
    /// <summary>
    /// Implementación del servicio de Invoice
    /// Maneja validaciones, sincronización de pagos y actualización de estados
    /// </summary>
    public class InvoiceService : IInvoiceService
    {
        private readonly AppDbContext _context;

        public InvoiceService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Valida que un Invoice sea consistente
        /// </summary>
        public async Task<(bool IsValid, List<string> Errors)> ValidateInvoiceAsync(Invoice invoice)
        {
            var errors = new List<string>();

            // Validación 1: TotalAmount debe ser positivo
            if (invoice.TotalAmount <= 0)
            {
                errors.Add("TotalAmount debe ser mayor a 0");
            }

            // Validación 2: PaidAmount no puede ser negativo
            if (invoice.PaidAmount < 0)
            {
                errors.Add("PaidAmount no puede ser negativo");
            }

            // Validación 3: PaidAmount no puede exceder TotalAmount
            if (invoice.PaidAmount > invoice.TotalAmount)
            {
                errors.Add($"PaidAmount ({invoice.PaidAmount}) no puede exceder TotalAmount ({invoice.TotalAmount})");
            }

            // Validación 4: Status debe ser consistente con PaidAmount
            var expectedStatus = CalculateStatus(invoice.TotalAmount, invoice.PaidAmount);
            if (invoice.Status != expectedStatus)
            {
                errors.Add($"Status inconsistente: es {invoice.Status} pero debería ser {expectedStatus}");
            }

            // Validación 5: Si está eliminado, no debería haber nuevos pagos
            if (invoice.IsDeleted && invoice.PaidAmount < invoice.TotalAmount)
            {
                errors.Add("No se pueden hacer pagos a una factura eliminada");
            }

            // Validación 6: DueDate debe ser posterior a CreatedAt
            if (invoice.DueDate.HasValue && invoice.DueDate < invoice.CreatedAt)
            {
                errors.Add("DueDate no puede ser anterior a CreatedAt");
            }

            return (errors.Count == 0, errors);
        }

        /// <summary>
        /// Actualiza PaidAmount basado en los pagos registrados
        /// Tambien actualiza el Status automáticamente
        /// </summary>
        public async Task<Invoice> UpdatePaidAmountAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

            if (invoice == null)
            {
                throw new ArgumentException($"Invoice {invoiceId} no encontrado o está eliminado");
            }

            // Calcular suma de pagos NO eliminados
            var totalPaid = invoice.Payments
                .Where(p => !p.IsDeleted)
                .Sum(p => p.AmountPaid);

            // Actualizar PaidAmount
            invoice.PaidAmount = totalPaid;

            // Actualizar Status basado en el nuevo PaidAmount
            invoice.Status = CalculateStatus(invoice.TotalAmount, invoice.PaidAmount);

            // Actualizar timestamp de auditoría
            invoice.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return invoice;
        }

        /// <summary>
        /// Calcula el status apropiado basado en los montos
        /// </summary>
        public InvoiceStatus CalculateStatus(decimal totalAmount, decimal paidAmount)
        {
            if (paidAmount <= 0)
                return InvoiceStatus.Pending;

            if (paidAmount >= totalAmount)
                return InvoiceStatus.Paid;

            return InvoiceStatus.PartiallyPaid;
        }

        /// <summary>
        /// Valida si se puede hacer un nuevo pago
        /// </summary>
        public async Task<(bool CanPay, string ErrorMessage)> CanAcceptPaymentAsync(int invoiceId, decimal paymentAmount)
        {
            var invoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.Id == invoiceId && !i.IsDeleted);

            if (invoice == null)
            {
                return (false, "Invoice no encontrado o está eliminado");
            }

            if (paymentAmount <= 0)
            {
                return (false, "El monto del pago debe ser mayor a 0");
            }

            var remainingAmount = invoice.RemainingAmount;

            if (remainingAmount <= 0)
            {
                return (false, "La factura ya está completamente pagada");
            }

            if (paymentAmount > remainingAmount)
            {
                return (false, $"El pago ({paymentAmount}) excede el balance pendiente ({remainingAmount})");
            }

            return (true, string.Empty);
        }
    }
}
