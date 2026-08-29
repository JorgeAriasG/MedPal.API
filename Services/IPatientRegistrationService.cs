using System.Threading.Tasks;
using MedPal.API.DTOs;

namespace MedPal.API.Services
{
    /// <summary>
    /// Casos de uso del registro del paciente ghost: completar registro con token single-use
    /// y reenvío del enlace. La transacción protege que el token solo se marque usado
    /// cuando el alta de credenciales del paciente persiste exitosamente.
    /// </summary>
    public interface IPatientRegistrationService
    {
        Task<PatientLoginResponseDTO> CompletePatientRegistrationAsync(CompletePatientRegistrationDTO dto);

        Task<string> ResendRegistrationAsync(ResendRegistrationDTO dto);
    }
}