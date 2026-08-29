using System.Threading.Tasks;

namespace MedPal.API.Services
{
    /// <summary>
    /// Envía por WhatsApp el enlace de completado de registro al paciente ghost.
    /// Se invoca DESPUÉS de confirmar la transacción del booking.
    /// </summary>
    public interface IRegistrationNotificationService
    {
        Task SendRegistrationLinkAsync(int patientId, string rawToken);
    }
}