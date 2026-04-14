using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LNUBookShare.UnitTests.Notification_tests
{
    public class Notification_tests
    {
        private readonly Mock<INotificationRepository> _notifRepoMock;
        private readonly Mock<ILogger<NotificationService>> _loggerMock;
        private readonly NotificationService _notifService;

        public Notification_tests()
        {
            _notifRepoMock = new Mock<INotificationRepository>();
            _loggerMock = new Mock<ILogger<NotificationService>>();
            _notifService = new NotificationService(_notifRepoMock.Object, _loggerMock.Object);
        }

        
        [Fact]
        public async Task CreateNotificationAsync_ShouldReturnSuccess_WhenValidData()
        {
            // Arrange
            int userId = 1;
            string message = "Test Message";

            // Act
            var result = await _notifService.CreateNotificationAsync(userId, message);

            // Assert
            Assert.True(result.IsSuccess);
            _notifRepoMock.Verify(r => r.AddAsync(It.Is<Notification>(n =>
                n.UserId == userId &&
                n.Message == message &&
                n.IsRead == false)), Times.Once);
        }

        [Fact]
        public async Task GetUserNotificationsAsync_ShouldReturnNotifications()
        {
            // Arrange
            int userId = 1;
            var notifications = new List<Notification>
    {
        new Notification { Id = 1, UserId = userId, Message = "Msg 1" }
    };
            _notifRepoMock.Setup(r => r.GetByUserIdAsync(userId)).ReturnsAsync(notifications);

            // Act
            var result = await _notifService.GetUserNotificationsAsync(userId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(notifications, result.Value);

            // Змінено на LogLevel.Debug, щоб відповідати коду сервісу
            _loggerMock.Verify(x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Отримано")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        }


        [Fact]
        public async Task MarkAsReadAsync_ShouldReturnFailure_WhenNotificationNotFound()
        {
            // Arrange
            _notifRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>()))
                .ReturnsAsync(default(Notification));

            // Act
            var result = await _notifService.MarkAsReadAsync(1, 1);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Сповіщення не знайдено.", result.Error);

            _loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Спроба видалити неіснуюче сповіщення")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_ShouldReturnFailure_WhenUserIsNotOwner()
        {
            // Arrange
            var notification = new Notification { Id = 1, UserId = 99 }; 
            _notifRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(notification);

            // Act
            var result = await _notifService.MarkAsReadAsync(1, 1); 

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Немає доступу до цього сповіщення.", result.Error);

            _loggerMock.Verify(x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("отримати доступ до чужого сповіщення")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        }

        [Fact]
        public async Task MarkAsReadAsync_ShouldDelete_WhenOwnerCalls()
        {
            // Arrange
            var notification = new Notification { Id = 1, UserId = 1 };
            _notifRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(notification);

            // Act
            var result = await _notifService.MarkAsReadAsync(1, 1);

            // Assert
            Assert.True(result.IsSuccess);
            _notifRepoMock.Verify(r => r.DeleteAsync(notification), Times.Once);

            _loggerMock.Verify(x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("успішно видалив")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
        }
    }
}
