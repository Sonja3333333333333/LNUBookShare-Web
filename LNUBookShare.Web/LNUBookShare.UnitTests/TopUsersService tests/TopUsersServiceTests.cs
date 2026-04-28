using LNUBookShare.Application.Services;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.TopUsersService_tests
{
    public class TopUsersServiceTests
    {
        private readonly Mock<IBookRepository> _bookRepoMock;
        private readonly Mock<ILogger<TopUsersService>> _loggerMock;
        private readonly TopUsersService _topUsersService;

        public TopUsersServiceTests()
        {
            _bookRepoMock = new Mock<IBookRepository>();
            _loggerMock = new Mock<ILogger<TopUsersService>>();
            _topUsersService = new TopUsersService(_bookRepoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetTopUsersOfMonthAsync_ShouldReturnTop5ActiveUsers_WithRecentBooks()
        {
            // Arrange
            var activeUser1 = new User { Id = 1, FirstName = "Іван", LastName = "Іванов", IsActive = true };
            var activeUser2 = new User { Id = 2, FirstName = "Петро", LastName = "Петров", IsActive = true };
            var blockedUser = new User { Id = 3, FirstName = "Злий", LastName = "Хакер", IsActive = false };

            var books = new List<Book>
            {
                new Book { BookId = 1, Owner = activeUser1, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                new Book { BookId = 2, Owner = activeUser1, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                new Book { BookId = 3, Owner = activeUser2, CreatedAt = DateTime.UtcNow.AddDays(-2) },
                new Book { BookId = 4, Owner = blockedUser, CreatedAt = DateTime.UtcNow.AddDays(-1) }, 
                new Book { BookId = 5, Owner = activeUser1, CreatedAt = DateTime.UtcNow.AddDays(-40) }
            };

            _bookRepoMock.Setup(r => r.GetAllBooksWithOwnersAsync()).ReturnsAsync(books);

            // Act
            var result = await _topUsersService.GetTopUsersOfMonthAsync();

            // Assert
            Assert.True(result.IsSuccess);
            var topUsers = result.Value.ToList();

            Assert.Equal(2, topUsers.Count); 

            Assert.Equal(1, topUsers[0].UserId);
            Assert.Equal(2, topUsers[0].AddedBooksCount); 

            Assert.Equal(2, topUsers[1].UserId);
            Assert.Equal(1, topUsers[1].AddedBooksCount);
        }

        [Fact]
        public async Task GetTopUsersOfMonthAsync_WhenNoRecentBooks_ShouldReturnEmptyList()
        {
            // Arrange
            var books = new List<Book>
            {
                new Book { BookId = 1, Owner = new User { IsActive = true }, CreatedAt = DateTime.UtcNow.AddDays(-40) }
            };

            _bookRepoMock.Setup(r => r.GetAllBooksWithOwnersAsync()).ReturnsAsync(books);

            // Act
            var result = await _topUsersService.GetTopUsersOfMonthAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }
    }
}