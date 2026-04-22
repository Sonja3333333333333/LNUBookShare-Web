using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.AdminBookService_tests
{
    public class AdminBookServiceDeleteTests
    {
        private readonly Mock<IBookRepository> _bookRepoMock;
        private readonly Mock<ILogger<AdminBookService>> _loggerMock;
        private readonly AdminBookService _service;

        public AdminBookServiceDeleteTests()
        {
            _bookRepoMock = new Mock<IBookRepository>();
            _loggerMock = new Mock<ILogger<AdminBookService>>();
            _service = new AdminBookService(_bookRepoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task DeleteBookAsync_WhenBookExists_ShouldReturnSuccess()
        {
            var book = new Book { BookId = 1, Title = "Тестова книга" };
            _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);

            var result = await _service.DeleteBookAsync(1);

            Assert.True(result.IsSuccess);
            _bookRepoMock.Verify(r => r.DeleteAsync(book), Times.Once);
        }

        [Fact]
        public async Task DeleteBookAsync_WhenBookNotFound_ShouldReturnFailure()
        {
            _bookRepoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((Book?)null);

            var result = await _service.DeleteBookAsync(99);

            Assert.True(result.IsFailure);
            Assert.Equal("Книгу не знайдено.", result.Error);
            _bookRepoMock.Verify(r => r.DeleteAsync(It.IsAny<Book>()), Times.Never);
        }

        [Fact]
        public async Task DeleteBookAsync_WhenRepositoryThrows_ShouldReturnFailure()
        {
            var book = new Book { BookId = 1, Title = "Тестова книга" };
            _bookRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(book);
            _bookRepoMock.Setup(r => r.DeleteAsync(book)).ThrowsAsync(new Exception("DB error"));

            var result = await _service.DeleteBookAsync(1);

            Assert.True(result.IsFailure);
            Assert.Equal("Помилка при видаленні книги.", result.Error);
        }
    }
}