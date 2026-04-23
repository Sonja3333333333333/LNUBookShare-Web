using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Models;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.Services
{
    public class AdminBookServiceTests
    {
        private readonly Mock<IBookRepository> _bookRepoMock;
        private readonly Mock<ILogger<AdminBookService>> _loggerMock;
        private readonly AdminBookService _service;

        public AdminBookServiceTests()
        {
            _bookRepoMock = new Mock<IBookRepository>();
            _loggerMock = new Mock<ILogger<AdminBookService>>();
            _service = new AdminBookService(_bookRepoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllBooksAsync_Success_ReturnsMappedData()
        {
            // Arrange
            var mockBooks = new List<Book>
            {
                new Book
                {
                    BookId = 1,
                    Title = "Чистий Код",
                    Author = "Роберт Мартін",
                    Owner = new User { FirstName = "Максим", LastName = "Богданович" }
                }
            };
            _bookRepoMock.Setup(r => r.GetAllBooksWithOwnersAsync())
                .ReturnsAsync(mockBooks);

            // Act
            var result = await _service.GetAllBooksAsync();

            // Assert
            Assert.True(result.IsSuccess);
            var dtos = result.Value.ToList();
            Assert.Single(dtos);
            Assert.Equal("Максим Богданович", dtos[0].OwnerName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)] 
        public async Task AdminSearchBooksAsync_EmptyQuery_ReturnsFailure(string? query)
        {

            // Act
            var result = await _service.AdminSearchBooksAsync("title", query!);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Пошуковий запит не може бути порожнім.", result.Error);
            _bookRepoMock.Verify(r => r.SearchBooksByCriteriaAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AdminSearchBooksAsync_ValidQuery_CallsRepoWithTrimmedText()
        {
            // Arrange
            string query = "  Кобзар  ";
            _bookRepoMock.Setup(r => r.SearchBooksByCriteriaAsync("title", "Кобзар"))
                .ReturnsAsync(new List<Book>());

            // Act
            await _service.AdminSearchBooksAsync("title", query);

            // Assert
            _bookRepoMock.Verify(r => r.SearchBooksByCriteriaAsync("title", "Кобзар"), Times.Once);
        }
    }
}