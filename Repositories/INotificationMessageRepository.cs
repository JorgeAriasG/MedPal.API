using MedPal.API.Models;
using MedPal.API.Enums;

namespace MedPal.API.Repositories
{
    public interface INotificationMessageRepository
    {
        Task<IEnumerable<NotificationMessage>> GetAllNotificationsAsync();
        Task<NotificationMessage> GetNotificationByIdAsync(int id);
        Task<IEnumerable<NotificationMessage>> GetNotificationsByUserIdAsync(int userId);
        Task<IEnumerable<NotificationMessage>> GetUnreadNotificationsByUserIdAsync(int userId);
        Task<IEnumerable<NotificationMessage>> GetNotificationsByTypeAsync(NotificationType type);
        Task<NotificationMessage> AddNotificationAsync(NotificationMessage notification);
        void UpdateNotification(NotificationMessage notification);
        void RemoveNotification(NotificationMessage notification);
        Task<int> CompleteAsync();
    }
}
