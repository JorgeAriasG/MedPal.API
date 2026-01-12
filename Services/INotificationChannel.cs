using System.Threading.Tasks;
using MedPal.API.Models;

namespace MedPal.API.Services
{
    public interface INotificationChannel
    {
        Task SendAsync(NotificationMessage message);
        bool CanHandle(NotificationType type);
    }
}
