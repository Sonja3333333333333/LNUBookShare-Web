using Castle.Core.Logging;
using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.Services
{
    public class BookStatusServiceTests
    {
        private readonly Mock<IProfileService> _profileServiceMock;
        private readonly Mock<IReservationService> _reservationServiceMock;
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<ILogger<BookStatusService>> _loggerMock;
        private readonly BookStatusService _service;

        public BookStatusServiceTests()
        {
            _profileServiceMock = new Mock<IProfileService>();
            _reservationServiceMock = new Mock<IReservationService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _loggerMock = new Mock<ILogger<BookStatusService>>();

            _service = new BookStatusService(
                _profileServiceMock.Object,
                _reservationServiceMock.Object,
                _notificationServiceMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task IssueBookAsync_WhenBookNotFound_ReturnsFailure()
        {
            int bookId = 1;
            int ownerId = 10;
            _profileServiceMock.Setup(s => s.GetUserBooksAsync(ownerId)).ReturnsAsync(Result<List<Book>>.Success(new List<Book>()));

            // Act
            var result = await _service.IssueBookAsync(bookId, ownerId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Книгу не знайдено.", result.Error);
            _profileServiceMock.Verify(s => s.UpdateBookAsync(It.IsAny<Book>()), Times.Never);
        }

        [Fact]
        public async Task IssueBookAsync_WhenBookFound_UpdatesStatusToBorrowed()
        {
            // Arrange
            int bookId = 1;
            int ownerId = 10;
            var book = new Book { BookId = bookId, Status = "reserved" };

            _profileServiceMock.Setup(s => s.GetUserBooksAsync(ownerId))
                .ReturnsAsync(Result<List<Book>>.Success(new List<Book> { book }));

            // Act
            var result = await _service.IssueBookAsync(bookId, ownerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("borrowed", book.Status);
            _profileServiceMock.Verify(s => s.UpdateBookAsync(book), Times.Once);
        }

        [Fact]
        public async Task ConfirmReturnAsync_WhenBookNotFound_ReturnsFailure()
        {
            // Arrange
            int bookId = 1;
            int ownerId = 10;
            _profileServiceMock.Setup(s => s.GetUserBooksAsync(ownerId))
                .ReturnsAsync(Result<List<Book>>.Failure("Error"));

            // Act
            var result = await _service.ConfirmReturnAsync(bookId, ownerId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Книгу не знайдено.", result.Error);
        }

        [Fact]
        public async Task ConfirmReturnAsync_WhenQueueIsEmpty_SetsStatusToAvailable()
        {
            // Arrange
            int bookId = 1;
            int ownerId = 10;
            var book = new Book { BookId = bookId, Status = "borrowed" };

            _profileServiceMock.Setup(s => s.GetUserBooksAsync(ownerId))
                .ReturnsAsync(Result<List<Book>>.Success(new List<Book> { book }));

            _reservationServiceMock.Setup(s => s.GetQueueUsersAsync(bookId))
                .ReturnsAsync(Result<List<User>>.Success(new List<User>()));

            // Act
            var result = await _service.ConfirmReturnAsync(bookId, ownerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("available", book.Status);
            _profileServiceMock.Verify(s => s.UpdateBookAsync(book), Times.Once);
            _notificationServiceMock.Verify(s => s.CreateNotificationAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task ConfirmReturnAsync_WhenQueueHasMultipleUsers_NotifiesNextUserAndSetsStatusReserved()
        {
            // Arrange
            int bookId = 1;
            int ownerId = 10;
            var book = new Book { BookId = bookId, Title = "Кобзар", Status = "borrowed" };

            var currentUser = new User { Id = 100, FirstName = "Максим" };
            var nextUser = new User { Id = 101, FirstName = "Софія" };
            var queue = new List<User> { currentUser, nextUser };

            _profileServiceMock.Setup(s => s.GetUserBooksAsync(ownerId))
                .ReturnsAsync(Result<List<Book>>.Success(new List<Book> { book }));

            _reservationServiceMock.Setup(s => s.GetQueueUsersAsync(bookId))
                .ReturnsAsync(Result<List<User>>.Success(queue));

            // Act
            var result = await _service.ConfirmReturnAsync(bookId, ownerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("reserved", book.Status);

            _reservationServiceMock.Verify(s => s.LeaveQueueAsync(bookId, currentUser.Id), Times.Once);

            _notificationServiceMock.Verify(s => s.CreateNotificationAsync(
                nextUser.Id,
                It.Is<string>(msg => msg.Contains("автоматично заброньована за вами")),
                bookId), Times.Once);

            _profileServiceMock.Verify(s => s.UpdateBookAsync(book), Times.Once);
        }
    }
}