using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.NotificationService_tests
{
    public class NotificationServiceTests
    {
        private readonly Mock<INotificationRepository> _notificationRepoMock;
        private readonly Mock<ILogger<NotificationService>> _loggerMock;
        private readonly NotificationService _notificationService;

        public NotificationServiceTests()
        {
            _notificationRepoMock = new Mock<INotificationRepository>();
            _loggerMock = new Mock<ILogger<NotificationService>>();

            _notificationService = new NotificationService(
                _notificationRepoMock.Object,
                _loggerMock.Object);
        }


        [Fact]
        public async Task MarkAsReadAsync_WhenNotificationExistsAndBelongsToUser_ShouldDeleteAndReturnSuccess()
        {
            int notificationId = 1;
            int userId = 10;
            var notification = new Notification { Id = notificationId, UserId = userId };

            _notificationRepoMock.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync(notification);

            var result = await _notificationService.MarkAsReadAsync(notificationId, userId);

            Assert.True(result.IsSuccess);

            _notificationRepoMock.Verify(r => r.DeleteAsync(notification), Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenNotificationDoesNotExist_ShouldReturnFailure()
        {
            int notificationId = 1;
            int userId = 10;

            _notificationRepoMock.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync((Notification?)null);

            var result = await _notificationService.MarkAsReadAsync(notificationId, userId);

            Assert.True(result.IsFailure);
            Assert.Equal("Сповіщення не знайдено.", result.Error);
            _notificationRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Notification>()), Times.Never);
        }

        [Fact]
        public async Task MarkAsReadAsync_WhenNotificationBelongsToAnotherUser_ShouldReturnFailure()
        {
            int notificationId = 1;
            int currentUserId = 10;
            int anotherUserId = 99; 
            var notification = new Notification { Id = notificationId, UserId = anotherUserId };

            _notificationRepoMock.Setup(r => r.GetByIdAsync(notificationId)).ReturnsAsync(notification);

            var result = await _notificationService.MarkAsReadAsync(notificationId, currentUserId);

            Assert.True(result.IsFailure);
            Assert.Equal("Немає доступу до цього сповіщення.", result.Error);
            _notificationRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Notification>()), Times.Never);
        }
    }
}