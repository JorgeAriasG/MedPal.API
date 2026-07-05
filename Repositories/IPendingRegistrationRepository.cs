using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Repositories
{
    public interface IPendingRegistrationRepository
    {
        Task<PendingRegistration> CreateAsync(PendingRegistration registration);
        Task<PendingRegistration?> GetBySessionIdAsync(string sessionId);
        Task UpdateStatusAsync(int id, string status, int? accountId = null);
        Task DeleteExpiredAsync();
    }
}
