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
            _logger.LogDebug("Отримано {Count} сповіщень для користувача {UserId}.", notifications.Count(), userId);

            return Result<IEnumerable<Notification>>.Success(notifications);
        }

        public async Task<Result> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _notificationRepository.GetByIdAsync(notificationId);

            if (notification == null)
            {
                _logger.LogWarning("Спроба видалити неіснуюче сповіщення {NotificationId} користувачем {UserId}.", notificationId, userId);
                return Result.Failure("Сповіщення не знайдено.");
            }

            if (notification.UserId != userId)
            {
                _logger.LogWarning("Користувач {UserId} намагався отримати доступ до чужого сповіщення {NotificationId}, яке належить користувачу {OwnerId}.", userId, notificationId, notification.UserId);
                return Result.Failure("Немає доступу до цього сповіщення.");
            }

            await _notificationRepository.DeleteAsync(notification);
            _logger.LogInformation("Користувач {UserId} успішно видалив сповіщення {NotificationId}.", userId, notificationId);

            return Result.Success();
        }
    }
}
