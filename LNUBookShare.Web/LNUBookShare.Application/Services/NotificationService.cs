using System.Collections.Generic; // ДЛЯ IEnumerable
using System.Linq;
using System.Threading.Tasks;    // ДЛЯ Task
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
        private readonly IRealTimeNotificationSender _realTimeSender;

        public NotificationService(
            INotificationRepository notificationRepository,
            ILogger<NotificationService> logger,
            IRealTimeNotificationSender realTimeSender)
        {
            _notificationRepository = notificationRepository;
            _logger = logger;
            _realTimeSender = realTimeSender;
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

            // Відправляємо миттєвий пуш через міст
            await _realTimeSender.SendToUserAsync(userId, message);

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

            if (notification == null || notification.UserId != userId)
            {
                return Result.Failure("Сповіщення не знайдено або немає доступу.");
            }

            await _notificationRepository.DeleteAsync(notification);
            return Result.Success();
        }
    }
}