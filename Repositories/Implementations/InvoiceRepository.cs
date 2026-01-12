using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Enums;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly AppDbContext _context;

        public InvoiceRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
        {
            return await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Appointment)
                .Include(i => i.Payments)
                .ToListAsync();
        }

        public async Task<Invoice> GetInvoiceByIdAsync(int id)
        {
            return await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Appointment)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByPatientIdAsync(int patientId)
        {
            return await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Appointment)
                .Include(i => i.Payments)
                .Where(i => i.PatientId == patientId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByAppointmentIdAsync(int appointmentId)
        {
            return await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Appointment)
                .Include(i => i.Payments)
                .Where(i => i.AppointmentId == appointmentId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByStatusAsync(InvoiceStatus status)
        {
            return await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Appointment)
                .Include(i => i.Payments)
                .Where(i => i.Status == status)
                .ToListAsync();
        }

        public async Task<Invoice> AddInvoiceAsync(Invoice invoice)
        {
            await _context.Invoices.AddAsync(invoice);
            return invoice;
        }

        public void UpdateInvoice(Invoice invoice)
        {
            _context.Invoices.Update(invoice);
        }

        public void RemoveInvoice(Invoice invoice)
        {
            _context.Invoices.Remove(invoice);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
