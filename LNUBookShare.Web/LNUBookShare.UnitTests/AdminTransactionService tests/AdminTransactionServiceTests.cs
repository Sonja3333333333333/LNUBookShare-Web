using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.AdminTransactionService_tests
{
    public class AdminTransactionServiceTests
    {
        private readonly Mock<IRentalTransactionRepository> _repoMock;
        private readonly Mock<ILogger<AdminTransactionService>> _loggerMock;
        private readonly AdminTransactionService _service;

        public AdminTransactionServiceTests()
        {
            _repoMock = new Mock<IRentalTransactionRepository>();
            _loggerMock = new Mock<ILogger<AdminTransactionService>>();

            _service = new AdminTransactionService(_repoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetTransactionsAsync_ShouldReturnSuccess_WithTransactions()
        {
            // Arrange
            var transactions = new List<RentalTransaction>
            {
                new RentalTransaction { Id = 1, BookId = 10 },
                new RentalTransaction { Id = 2, BookId = 11 }
            };

            _repoMock.Setup(r => r.GetAllWithDetailsAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>()))
                .ReturnsAsync(transactions);

            // Act
            var result = await _service.GetTransactionsAsync("book", "Шевченко", "date_desc", "active");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.Count());
        }

        [Fact]
        public async Task GetTransactionsAsync_ShouldPassCorrectParametersToRepository()
        {
            // Arrange
            var expectedSearchBy = "owner";
            var expectedQuery = "Іван";
            var expectedSortBy = "created";
            var expectedStatus = "all";

            _repoMock.Setup(r => r.GetAllWithDetailsAsync(expectedSearchBy, expectedQuery, expectedSortBy, expectedStatus))
                .ReturnsAsync(new List<RentalTransaction>());

            // Act
            await _service.GetTransactionsAsync(expectedSearchBy, expectedQuery, expectedSortBy, expectedStatus);

            // Assert
            _repoMock.Verify(r => r.GetAllWithDetailsAsync(expectedSearchBy, expectedQuery, expectedSortBy, expectedStatus), Times.Once);
        }

        [Fact]
        public async Task GetTransactionsAsync_ShouldReturnFailure_WhenExceptionIsThrown()
        {
            // Arrange
            _repoMock.Setup(r => r.GetAllWithDetailsAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<string?>()))
                .ThrowsAsync(new Exception("Критична помилка бази даних"));

            // Act
            var result = await _service.GetTransactionsAsync("book", null, null, null);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Не вдалося завантажити список транзакцій.", result.Error);
        }
    }
}