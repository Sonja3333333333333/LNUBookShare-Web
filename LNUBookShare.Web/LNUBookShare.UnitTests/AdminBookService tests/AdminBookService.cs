using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.Tests.Services
{
    public class AdminBookServiceTests
    {
        private readonly Mock<IBookRepository> _bookRepositoryMock;
        private readonly Mock<ILogger<AdminBookService>> _loggerMock;
        private readonly AdminBookService _service;

        public AdminBookServiceTests()
        {
            _bookRepositoryMock = new Mock<IBookRepository>();
            _loggerMock = new Mock<ILogger<AdminBookService>>();
            _service = new AdminBookService(_bookRepositoryMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllBooksAsync_ShouldReturnSuccess_WhenBooksExist()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book
                {
                    BookId = 1,
                    Title = "Clean Code",
                    Author = "Robert Martin",
                    Owner = new User { FirstName = "Ivan", LastName = "Ivanov", Email = "ivan@test.com" }
                }
            };
            _bookRepositoryMock.Setup(repo => repo.GetAllBooksWithOwnersAsync()).ReturnsAsync(books);

            // Act
            var result = await _service.GetAllBooksAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var bookDto = result.Value.First();
            Assert.Equal("Clean Code", bookDto.Title);
            Assert.Equal("Ivan Ivanov", bookDto.OwnerName);
        }

        [Fact]
        public async Task GetAllBooksAsync_ShouldReturnFailure_WhenRepositoryReturnsNull()
        {
            // Arrange
            _bookRepositoryMock.Setup(repo => repo.GetAllBooksWithOwnersAsync()).ReturnsAsync((IEnumerable<Book>)null!);

            // Act
            var result = await _service.GetAllBooksAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Дані про книги відсутні.", result.Error);
        }

        [Fact]
        public async Task AdminSearchBooksAsync_ShouldReturnSuccess_WhenQueryIsValid()
        {
            // Arrange
            string searchBy = "Title";
            string query = "Clean";
            var books = new List<Book>
            {
                new Book { BookId = 2, Title = "Clean Architecture", Author = "Robert Martin" }
            };

            _bookRepositoryMock.Setup(repo => repo.SearchBooksByCriteriaAsync(searchBy, query))
                .ReturnsAsync(books);

            // Act
            var result = await _service.AdminSearchBooksAsync(searchBy, query);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotEmpty(result.Value);
            _bookRepositoryMock.Verify(r => r.SearchBooksByCriteriaAsync(searchBy, query), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task AdminSearchBooksAsync_ShouldReturnFailure_WhenQueryIsInvalid(string? invalidQuery)
        {
            // Act
            var result = await _service.AdminSearchBooksAsync("Title", invalidQuery!);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Пошуковий запит не може бути порожнім.", result.Error);

            _bookRepositoryMock.Verify(r => r.SearchBooksByCriteriaAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AdminSearchBooksAsync_ShouldTrimQueryBeforeCallingRepository()
        {
            // Arrange
            string searchBy = "Author";
            string queryWithSpaces = "  Martin  ";
            _bookRepositoryMock.Setup(repo => repo.SearchBooksByCriteriaAsync(searchBy, "Martin"))
                .ReturnsAsync(new List<Book>());

            // Act
            await _service.AdminSearchBooksAsync(searchBy, queryWithSpaces);

            // Assert
            _bookRepositoryMock.Verify(r => r.SearchBooksByCriteriaAsync(searchBy, "Martin"), Times.Once);
        }

        [Fact]
        public async Task MapToAdminBookDto_ShouldHandleMissingOwner()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { BookId = 10, Title = "Unit Testing", Owner = null! }
            };
            _bookRepositoryMock.Setup(repo => repo.GetAllBooksWithOwnersAsync()).ReturnsAsync(books);

            // Act
            var result = await _service.GetAllBooksAsync();

            // Assert
            var dto = result.Value.First();
            Assert.Equal("Невідомо", dto.OwnerName);
            Assert.Equal("Невідомо", dto.OwnerEmail);
        }
    }
}