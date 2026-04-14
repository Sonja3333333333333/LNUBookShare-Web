using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.ReservationService_tests
{
    public class ReservationServiceTests
    {
        private readonly Mock<IReservationRepository> _reservationRepoMock;
        private readonly Mock<IBookRepository> _bookRepoMock;
        private readonly Mock<INotificationRepository> _notificationRepoMock; // НОВЕ: Мок для сповіщень
        private readonly Mock<ILogger<ReservationService>> _loggerMock;
        private readonly ReservationService _reservationService;

        public ReservationServiceTests()
        {
            _reservationRepoMock = new Mock<IReservationRepository>();
            _bookRepoMock = new Mock<IBookRepository>();
            _notificationRepoMock = new Mock<INotificationRepository>();
            _loggerMock = new Mock<ILogger<ReservationService>>();

            _reservationService = new ReservationService(
                _reservationRepoMock.Object,
                _bookRepoMock.Object,
                _notificationRepoMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task ReserveBookAsync_WhenBookIsAvailable_ShouldChangeStatusToReserved()
        {
            // Arrange
            var book = new Book { BookId = 1, Status = "available", OwnerId = 99 };
            _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

            // Act
            var result = await _reservationService.ReserveBookAsync(1, 10);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("reserved", book.Status);
            _bookRepoMock.Verify(r => r.UpdateAsync(book), Times.Once);
            _reservationRepoMock.Verify(r => r.AddAsync(It.IsAny<ReservationQueue>()), Times.Once);
        }

        [Fact]
        public async Task ReserveBookAsync_WhenUserIsOwner_ShouldReturnFailure()
        {
            // Arrange
            var book = new Book { BookId = 1, Status = "available", OwnerId = 10 };
            _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

            // Act
            var result = await _reservationService.ReserveBookAsync(1, 10);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Ви не можете бронювати власну книгу.", result.Error);
        }

        [Fact]
        public async Task JoinQueueAsync_WhenBookIsAvailable_ShouldReturnFailure_BecauseShouldUseReserve()
        {
            // Arrange
            var book = new Book { BookId = 1, Status = "available" };
            _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

            // Act
            var result = await _reservationService.JoinQueueAsync(1, 10);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Contains("Використовуйте бронювання", result.Error);
        }

        [Fact]
        public async Task JoinQueueAsync_WhenUserAlreadyInQueue_ShouldReturnFailure()
        {
            // Arrange
            var book = new Book { BookId = 1, Status = "reserved" };
            _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
            _reservationRepoMock.Setup(r => r.ExistsAsync(1, 10)).ReturnsAsync(true);

            // Act
            var result = await _reservationService.JoinQueueAsync(1, 10);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Ви вже у черзі.", result.Error);
        }


        [Fact]
        public async Task ReserveBookAsync_WhenSuccessful_ShouldCreateNotificationForOwner()
        {
            // Arrange
            int bookId = 1;
            int readerId = 10;
            int ownerId = 99;

            var book = new Book { BookId = bookId, Status = "available", OwnerId = ownerId };
            _bookRepoMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);

            // Act
            var result = await _reservationService.ReserveBookAsync(bookId, readerId);

            // Assert
            Assert.True(result.IsSuccess);

            _notificationRepoMock.Verify(n => n.AddAsync(It.Is<Notification>(
                notif => notif.UserId == ownerId &&
                         notif.BookId == bookId &&
                         notif.IsRead == false
            )), Times.Once);
        }

        [Fact]
        public async Task ReserveBookAsync_WhenFails_ShouldNotCreateNotification()
        {
            // Arrange
            int bookId = 1;
            int ownerId = 10;

            var book = new Book { BookId = bookId, Status = "available", OwnerId = ownerId };
            _bookRepoMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);

            // Act
            var result = await _reservationService.ReserveBookAsync(bookId, ownerId);

            // Assert
            Assert.True(result.IsFailure);

            _notificationRepoMock.Verify(n => n.AddAsync(It.IsAny<Notification>()), Times.Never);
        }
    }
}