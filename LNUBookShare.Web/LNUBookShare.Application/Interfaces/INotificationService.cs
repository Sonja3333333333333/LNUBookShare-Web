using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface INotificationService
    {
        Task<Result> CreateNotificationAsync(int userId, string message, int? bookId = null);
        Task<Result<IEnumerable<Notification>>> GetUserNotificationsAsync(int userId);
        Task<Result> MarkAsReadAsync(int notificationId, int userId);
    }
}