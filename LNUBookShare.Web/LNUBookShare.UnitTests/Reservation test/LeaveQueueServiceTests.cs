using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.ReservationService_tests
{
    public class LeaveQueueServiceTests
    {
        private readonly Mock<IReservationRepository> _reservationRepoMock;
        private readonly Mock<IBookRepository> _bookRepoMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<ILogger<ReservationService>> _loggerMock;
        private readonly ReservationService _reservationService;

        public LeaveQueueServiceTests()
        {
            _reservationRepoMock = new Mock<IReservationRepository>();
            _bookRepoMock = new Mock<IBookRepository>();
            _notificationServiceMock = new Mock<INotificationService>();
            _loggerMock = new Mock<ILogger<ReservationService>>();

            _reservationService = new ReservationService(
                _reservationRepoMock.Object,
                _bookRepoMock.Object,
                _notificationServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task LeaveQueueAsync_WhenUserIsInQueue_ShouldReturnSuccess()
        {
            
            int bookId = 1;
            int userId = 10;

            var queueItem = new ReservationQueue
            {
                QueueId = 1,
                BookId = bookId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            };

            var usersInQueue = new List<User>
            {
                new User { Id = userId, FirstName = "Тест" },
            };

            _reservationRepoMock
                .Setup(r => r.GetQueueUsersAsync(bookId))
                .ReturnsAsync(usersInQueue);

            _reservationRepoMock
                .Setup(r => r.GetByUserAndBookAsync(userId, bookId))
                .ReturnsAsync(queueItem);

           
            var result = await _reservationService.LeaveQueueAsync(bookId, userId);

            
            Assert.True(result.IsSuccess);
            _reservationRepoMock.Verify(r => r.DeleteAsync(queueItem), Times.Once);
        }

        [Fact]
        public async Task LeaveQueueAsync_WhenUserIsNotInQueue_ShouldReturnFailure()
        {
            
            int bookId = 1;
            int userId = 99;

            _reservationRepoMock
                .Setup(r => r.GetQueueUsersAsync(bookId))
                .ReturnsAsync(new List<User>());

            _reservationRepoMock
                .Setup(r => r.GetByUserAndBookAsync(userId, bookId))
                .ReturnsAsync((ReservationQueue?)null);

            
            var result = await _reservationService.LeaveQueueAsync(bookId, userId);

           
            Assert.True(result.IsFailure);
            Assert.Equal("Користувача не знайдено в черзі.", result.Error);
            _reservationRepoMock.Verify(r => r.DeleteAsync(It.IsAny<ReservationQueue>()), Times.Never);
        }

        [Fact]
        public async Task LeaveQueueAsync_WhenCalled_ShouldNotDeleteOtherUsersFromQueue()
        {
            
            int bookId = 1;
            int userId = 10;
            int otherUserId = 20;

            var queueItem = new ReservationQueue
            {
                QueueId = 1,
                BookId = bookId,
                UserId = userId,
            };

            _reservationRepoMock
                .Setup(r => r.GetQueueUsersAsync(bookId))
                .ReturnsAsync(new List<User>
                {
                    new User { Id = userId },
                    new User { Id = otherUserId },
                });

            _reservationRepoMock
                .Setup(r => r.GetByUserAndBookAsync(userId, bookId))
                .ReturnsAsync(queueItem);

            
            var result = await _reservationService.LeaveQueueAsync(bookId, userId);

            
            Assert.True(result.IsSuccess);
            _reservationRepoMock.Verify(r => r.DeleteAsync(
                It.Is<ReservationQueue>(q => q.UserId == userId)), Times.Once);
            _reservationRepoMock.Verify(r => r.DeleteAsync(
                It.Is<ReservationQueue>(q => q.UserId == otherUserId)), Times.Never);
        }

        [Fact]
        public async Task LeaveQueueAsync_WhenFirstUserLeavesAndOthersRemain_ShouldNotifyNextUser()
        {
            // Arrange
            int bookId = 1;
            int leavingUserId = 10;
            int nextUserId = 20;

            var queueItem = new ReservationQueue { QueueId = 1, BookId = bookId, UserId = leavingUserId };
            var book = new Book { BookId = bookId, Title = "Кобзар", Status = "reserved" };

            var usersInQueue = new List<User>
            {
                new User { Id = leavingUserId, FirstName = "Максим", LastName = "Б" },
                new User { Id = nextUserId, FirstName = "Софія", LastName = "В" }
            };

            _reservationRepoMock.Setup(r => r.GetQueueUsersAsync(bookId)).ReturnsAsync(usersInQueue);
            _reservationRepoMock.Setup(r => r.GetByUserAndBookAsync(leavingUserId, bookId)).ReturnsAsync(queueItem);
            _bookRepoMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);

            var result = await _reservationService.LeaveQueueAsync(bookId, leavingUserId);

            Assert.True(result.IsSuccess);

            _notificationServiceMock.Verify(n => n.CreateNotificationAsync(
                nextUserId,
                It.Is<string>(msg => msg.Contains("автоматично заброньована за вами")),
                bookId), Times.Once);

            _bookRepoMock.Verify(b => b.UpdateAsync(It.IsAny<Book>()), Times.Never);
        }

        [Fact]
        public async Task LeaveQueueAsync_WhenLastUserLeaves_ShouldMakeBookAvailable()
        {
            // Arrange
            int bookId = 1;
            int userId = 10;

            var queueItem = new ReservationQueue { QueueId = 1, BookId = bookId, UserId = userId };
            var book = new Book { BookId = bookId, Title = "Кобзар", Status = "reserved" };

            var usersInQueue = new List<User>
            {
                new User { Id = userId, FirstName = "Максим" }
            };

            _reservationRepoMock.Setup(r => r.GetQueueUsersAsync(bookId)).ReturnsAsync(usersInQueue);
            _reservationRepoMock.Setup(r => r.GetByUserAndBookAsync(userId, bookId)).ReturnsAsync(queueItem);
            _bookRepoMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);

            // Act
            var result = await _reservationService.LeaveQueueAsync(bookId, userId);

            // Assert
            Assert.True(result.IsSuccess);

            Assert.Equal("available", book.Status);
            _bookRepoMock.Verify(b => b.UpdateAsync(book), Times.Once);

            _notificationServiceMock.Verify(n => n.CreateNotificationAsync(
                It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }
    }
}