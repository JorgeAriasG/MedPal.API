using System.Threading.Tasks;
using MedPal.API.DTOs;

namespace MedPal.API.Services
{
    public interface IRegistrationService
    {
        Task<InitiateRegistrationResponseDTO> InitiateAsync(InitiateRegistrationRequestDTO request);
        Task<CompleteRegistrationResponseDTO> CompleteAsync(string sessionId);
        Task<CompleteRegistrationResponseDTO?> CompleteFromWebhookAsync(string sessionId);
    }
}
