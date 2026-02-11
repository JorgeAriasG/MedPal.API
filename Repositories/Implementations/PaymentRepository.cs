using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class PaymentRepository : TenantAwareRepository<Payment>, IPaymentRepository
    {
        public PaymentRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync()
        {
            return await ApplyTenantFilter(_context.Payments
                .Include(p => p.Invoice))
                .ToListAsync();
        }

        public async Task<Payment> GetPaymentByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.Payments
                .Include(p => p.Invoice))
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByInvoiceIdAsync(int invoiceId)
        {
            return await ApplyTenantFilter(_context.Payments
                .Include(p => p.Invoice)
                .Where(p => p.InvoiceId == invoiceId))
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByPatientIdAsync(int patientId)
        {
            return await ApplyTenantFilter(_context.Payments
                .Include(p => p.Invoice)
                .Where(p => p.Invoice.PatientId == patientId))
                .ToListAsync();
        }

        public async Task<Payment> AddPaymentAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
            return payment;
        }

        public void UpdatePayment(Payment payment)
        {
            _context.Payments.Update(payment);
        }

        public void RemovePayment(Payment payment)
        {
            _context.Payments.Remove(payment);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
