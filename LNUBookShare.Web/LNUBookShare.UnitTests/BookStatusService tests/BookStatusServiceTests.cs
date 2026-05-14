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
            int bookId = 1;
            int ownerId = 10;
            _profileServiceMock.Setup(s => s.GetUserBooksAsync(ownerId))
                .ReturnsAsync(Result<List<Book>>.Success(new List<Book>()));

            var result = await _service.IssueBookAsync(bookId, ownerId);

            Assert.True(result.IsFailure);
            Assert.Equal("Книгу не знайдено.", result.Error);

            // Виправляємо типи параметрів для Verify
            _profileServiceMock.Verify(s => s.UpdateBookAsync(
                It.IsAny<int>(),      // ownerId
                It.IsAny<int>(),      // bookId
                It.IsAny<string>(),   // title
                It.IsAny<string>(),   // author
                It.IsAny<int>(),      // categoryId
                It.IsAny<string>(),   // description
                It.IsAny<string>(),   // isbn
                It.IsAny<string>(),   // status
                It.IsAny<int>(),      // facultyId
                It.IsAny<Image?>()    // avatar (Image об'єкт, а не int?)
            ), Times.Never);
        }

        [Fact]
        public async Task IssueBookAsync_WhenBookFound_UpdatesStatusToBorrowed()
        {
            int bookId = 1;
            int ownerId = 10;
            var book = new Book { BookId = bookId, Status = "reserved" };

            _profileServiceMock.Setup(s => s.GetUserBooksAsync(ownerId))
                .ReturnsAsync(Result<List<Book>>.Success(new List<Book> { book }));

            _reservationServiceMock.Setup(s => s.GetQueueUsersAsync(bookId))
                .ReturnsAsync(Result<List<User>>.Success(new List<User> { new User { Id = 100 } }));

            var result = await _service.IssueBookAsync(bookId, ownerId);

            Assert.True(result.IsSuccess);
            Assert.Equal("borrowed", book.Status);

            _profileServiceMock.Verify(s => s.UpdateBookAsync(
                ownerId,
                bookId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "borrowed",           // статус перевіряємо чітко
                It.IsAny<int>(),
                It.IsAny<Image?>()    // Image? замість int?
            ), Times.Once);
        }

        [Fact]
        public async Task ConfirmReturnAsync_WhenQueueIsEmpty_SetsStatusToAvailable()
        {
            int bookId = 1;
            int ownerId = 10;
            var book = new Book { BookId = bookId, Status = "borrowed" };

            _profileServiceMock.Setup(s => s.GetUserBooksAsync(ownerId))
                .ReturnsAsync(Result<List<Book>>.Success(new List<Book> { book }));

            _reservationServiceMock.Setup(s => s.GetQueueUsersAsync(bookId))
                .ReturnsAsync(Result<List<User>>.Success(new List<User>()));

            var activeTransaction = new RentalTransaction { Id = 1, BookId = bookId, Status = "active" };
            _transactionRepoMock.Setup(r => r.GetActiveByBookIdAsync(bookId))
                .ReturnsAsync(activeTransaction);

            var result = await _service.ConfirmReturnAsync(bookId, ownerId);

            Assert.True(result.IsSuccess);
            Assert.Equal("available", book.Status);

            _profileServiceMock.Verify(s => s.UpdateBookAsync(
                ownerId,
                bookId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "available",
                It.IsAny<int>(),
                It.IsAny<Image?>()    // Image? замість int?
            ), Times.Once);

            _transactionRepoMock.Verify(r => r.UpdateAsync(It.IsAny<RentalTransaction>()), Times.Once);
        }

        [Fact]
        public async Task ConfirmReturnAsync_WhenQueueHasMultipleUsers_NotifiesNextUserAndSetsStatusReserved()
        {
            int bookId = 1;
            int ownerId = 10;
            var book = new Book { BookId = bookId, Title = "Кобзар", Status = "borrowed" };
            var currentUser = new User { Id = 100 };
            var nextUser = new User { Id = 101 };

            _profileServiceMock.Setup(s => s.GetUserBooksAsync(ownerId))
                .ReturnsAsync(Result<List<Book>>.Success(new List<Book> { book }));

            _reservationServiceMock.Setup(s => s.GetQueueUsersAsync(bookId))
                .ReturnsAsync(Result<List<User>>.Success(new List<User> { currentUser, nextUser }));

            _transactionRepoMock.Setup(r => r.GetActiveByBookIdAsync(bookId))
                .ReturnsAsync(new RentalTransaction { Id = 2, Status = "active" });

            var result = await _service.ConfirmReturnAsync(bookId, ownerId);

            Assert.True(result.IsSuccess);
            Assert.Equal("reserved", book.Status);

            _reservationServiceMock.Verify(s => s.LeaveQueueAsync(bookId, currentUser.Id), Times.Once);
            _notificationServiceMock.Verify(s => s.CreateNotificationAsync(nextUser.Id, It.IsAny<string>(), bookId), Times.Once);

            _profileServiceMock.Verify(s => s.UpdateBookAsync(
                ownerId,
                bookId,
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<int>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                "reserved",
                It.IsAny<int>(),
                It.IsAny<Image?>()    // Image? замість int?
            ), Times.Once);
        }
    }
}