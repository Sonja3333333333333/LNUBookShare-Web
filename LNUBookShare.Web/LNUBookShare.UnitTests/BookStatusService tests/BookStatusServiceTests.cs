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
        private readonly Mock<IRentalTransactionRepository> _transactionRepoMock;
        private readonly Mock<IBookRepository> _bookRepoMock;
        private readonly Mock<ILogger<BookStatusService>> _loggerMock;
        private readonly BookStatusService _service;

        public BookStatusServiceTests()
        {
            _profileServiceMock = new Mock<IProfileService>();
            _reservationServiceMock = new Mock<IReservationService>();
            _notificationServiceMock = new Mock<INotificationService>();
            _transactionRepoMock = new Mock<IRentalTransactionRepository>();
            _bookRepoMock = new Mock<IBookRepository>();
            _loggerMock = new Mock<ILogger<BookStatusService>>();

            _service = new BookStatusService(
                _profileServiceMock.Object,
                _reservationServiceMock.Object,
                _notificationServiceMock.Object,
                _loggerMock.Object,
                _transactionRepoMock.Object,
                _bookRepoMock.Object);
        }

        [Fact]
        public async Task IssueBookAsync_WhenBookNotFound_ReturnsFailure()
        {
            // Arrange
            int bookId = 1, ownerId = 10;
            _profileServiceMock.Setup(s => s.GetUserBooksAsync(ownerId))
                .ReturnsAsync(Result<List<Book>>.Success(new List<Book>()));

            // Act
            var result = await _service.IssueBookAsync(bookId, ownerId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Книгу не знайдено.", result.Error);
        }

        [Fact]
        public async Task IssueBookAsync_WhenQueueIsEmpty_ReturnsFailure()
        {
            // Arrange
            int bookId = 1, ownerId = 10;
            var book = new Book { BookId = bookId };
            _profileServiceMock.Setup(s => s.GetUserBooksAsync(ownerId))
                .ReturnsAsync(Result<List<Book>>.Success(new List<Book> { book }));
            _reservationServiceMock.Setup(s => s.GetQueueUsersAsync(bookId))
                .ReturnsAsync(Result<List<User>>.Success(new List<User>()));

            // Act
            var result = await _service.IssueBookAsync(bookId, ownerId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Немає користувачів у черзі, кому можна видати книгу.", result.Error);
        }

        [Fact]
        public async Task IssueBookAsync_Success_CreatesTransactionAndUpdatesStatus()
        {
            // Arrange
            int bookId = 1, ownerId = 10;
            var book = new Book { BookId = bookId, Status = "available" };
            var borrower = new User { Id = 99 };

            _profileServiceMock.Setup(s => s.GetUserBooksAsync(ownerId))
                .ReturnsAsync(Result<List<Book>>.Success(new List<Book> { book }));
            _reservationServiceMock.Setup(s => s.GetQueueUsersAsync(bookId))
                .ReturnsAsync(Result<List<User>>.Success(new List<User> { borrower }));

            // Act
            var result = await _service.IssueBookAsync(bookId, ownerId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("borrowed", book.Status);
            _transactionRepoMock.Verify(r => r.AddAsync(It.Is<RentalTransaction>(t =>
                t.BorrowerId == borrower.Id && t.BookId == bookId)), Times.Once);
            _bookRepoMock.Verify(r => r.UpdateAsync(book), Times.Once);
        }

        [Fact]
        public async Task ConfirmReturnAsync_WhenQueueNotEmpty_NotifiesNextUser()
        {
            // Arrange
            int bookId = 1, ownerId = 10;
            var book = new Book { BookId = bookId, Title = "Кобзар" };
            var currentUser = new User { Id = 100 };
            var nextUser = new User { Id = 101 };

            _profileServiceMock.Setup(s => s.GetUserBooksAsync(ownerId))
                .ReturnsAsync(Result<List<Book>>.Success(new List<Book> { book }));
            _reservationServiceMock.Setup(s => s.GetQueueUsersAsync(bookId))
                .ReturnsAsync(Result<List<User>>.Success(new List<User> { currentUser, nextUser }));
            _transactionRepoMock.Setup(r => r.GetActiveByBookIdAsync(bookId))
                .ReturnsAsync(new RentalTransaction { Id = 5 });

            // Act
            await _service.ConfirmReturnAsync(bookId, ownerId);

            // Assert
            Assert.Equal("reserved", book.Status);
            _reservationServiceMock.Verify(s => s.LeaveQueueAsync(bookId, currentUser.Id), Times.Once);
            _notificationServiceMock.Verify(s => s.CreateNotificationAsync(nextUser.Id, It.IsAny<string>(), bookId), Times.Once);
        }
    }
}