using MedPal.API.Data;
using MedPal.API.Models;
using MedPal.API.Enums;
using MedPal.API.Services;
using Microsoft.EntityFrameworkCore;

namespace MedPal.API.Repositories.Implementations
{
    public class NotificationMessageRepository : TenantAwareRepository<NotificationMessage>, INotificationMessageRepository
    {
        public NotificationMessageRepository(AppDbContext context, ITenantContextService tenantContext)
            : base(context, tenantContext)
        {
        }

        public async Task<IEnumerable<NotificationMessage>> GetAllNotificationsAsync()
        {
            return await ApplyTenantFilter(_context.NotificationMessages
                .Where(n => !n.IsDeleted))
                .ToListAsync();
        }

        public async Task<NotificationMessage> GetNotificationByIdAsync(int id)
        {
            return await ApplyTenantFilter(_context.NotificationMessages)
                .FirstOrDefaultAsync(n => n.Id == id && !n.IsDeleted);
        }

        public async Task<IEnumerable<NotificationMessage>> GetNotificationsByUserIdAsync(int userId)
        {
            return await ApplyTenantFilter(_context.NotificationMessages
                .Where(n => n.UserId == userId && !n.IsDeleted))
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationMessage>> GetUnreadNotificationsByUserIdAsync(int userId)
        {
            return await ApplyTenantFilter(_context.NotificationMessages
                .Where(n => n.UserId == userId && !n.IsRead && !n.IsDeleted))
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<NotificationMessage>> GetNotificationsByTypeAsync(NotificationType type)
        {
            return await ApplyTenantFilter(_context.NotificationMessages
                .Where(n => n.Type == type && !n.IsDeleted))
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<NotificationMessage> AddNotificationAsync(NotificationMessage notification)
        {
            await _context.NotificationMessages.AddAsync(notification);
            return notification;
        }

        public void UpdateNotification(NotificationMessage notification)
        {
            _context.NotificationMessages.Update(notification);
        }

        public void RemoveNotification(NotificationMessage notification)
        {
            _context.NotificationMessages.Remove(notification);
        }

        public async Task<int> CompleteAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
