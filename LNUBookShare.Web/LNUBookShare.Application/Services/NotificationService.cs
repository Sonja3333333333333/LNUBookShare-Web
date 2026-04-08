using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    [Authorize]
    public class NotificationService : INotificationService
    {
        private readonly INotificationRepository _notificationRepository;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(INotificationRepository notificationRepository, ILogger<NotificationService> logger)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
        }

        public async Task<Result> CreateNotificationAsync(int userId, string message, int? bookId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                BookId = bookId,
                CreatedAt = DateTime.UtcNow,
                IsRead = false,
            };

            await _notificationRepository.AddAsync(notification);
            _logger.LogInformation("Створено сповіщення для користувача {UserId}: {Message}", userId, message);

            return Result.Success();
        }

        public async Task<Result<IEnumerable<Notification>>> GetUserNotificationsAsync(int userId)
        {
            var notifications = await _notificationRepository.GetByUserIdAsync(userId);
            return Result<IEnumerable<Notification>>.Success(notifications);
        }

        public async Task<Result> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);

            if (notification == null)
            {
                return Result.Failure("Сповіщення не знайдено.");
            }

            if (notification.UserId != userId)
            {
                return Result.Failure("Немає доступу до цього сповіщення.");
            }

            notification.IsRead = true;
            await _notificationRepository.UpdateAsync(notification);

            return Result.Success();
        }
    }
}
