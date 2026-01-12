using System.Threading.Tasks;
using System.Collections.Generic;
using MedPal.API.Models;

namespace MedPal.API.Services
{
    public interface IArcoService
    {
        Task<ArcoRequest> CreateRequestAsync(ArcoRequest request);
        Task<object> ExportUserDataAsync(int patientId);
        Task AnonymizePatientAsync(int patientId);
        Task ToggleMarketingBlockAsync(int patientId, bool blocked);
        Task<IEnumerable<ArcoRequest>> GetPendingRequestsAsync();
    }
}
