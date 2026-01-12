using MedPal.API.Models;
using MedPal.API.Enums;

namespace MedPal.API.Repositories
{
    public interface IInvoiceRepository
    {
        Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
        Task<Invoice> GetInvoiceByIdAsync(int id);
        Task<IEnumerable<Invoice>> GetInvoicesByPatientIdAsync(int patientId);
        Task<IEnumerable<Invoice>> GetInvoicesByAppointmentIdAsync(int appointmentId);
        Task<IEnumerable<Invoice>> GetInvoicesByStatusAsync(InvoiceStatus status);
        Task<Invoice> AddInvoiceAsync(Invoice invoice);
        void UpdateInvoice(Invoice invoice);
        void RemoveInvoice(Invoice invoice);
        Task<int> CompleteAsync();
    }
}
