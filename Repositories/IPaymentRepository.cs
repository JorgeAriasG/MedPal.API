using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IPaymentRepository
    {
        Task<IEnumerable<Payment>> GetAllPaymentsAsync();
        Task<Payment> GetPaymentByIdAsync(int id);
        Task<IEnumerable<Payment>> GetPaymentsByInvoiceIdAsync(int invoiceId);
        Task<IEnumerable<Payment>> GetPaymentsByPatientIdAsync(int patientId);
        Task<Payment> AddPaymentAsync(Payment payment);
        void UpdatePayment(Payment payment);
        void RemovePayment(Payment payment);
        Task<int> CompleteAsync();
    }
}
