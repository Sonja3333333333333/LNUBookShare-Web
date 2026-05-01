using LNUBookShare.Application.Services;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;

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
        public async Task GetTopUsersOfMonthAsync_ShouldReturnTopUsers_MappedCorrectly()
        {
            // Arrange
            var mockData = new List<TopUserDto>
            {
                new TopUserDto { UserId = 1, FullName = "Іван Іванов", AddedBooksCount = 3 },
                new TopUserDto { UserId = 2, FullName = "Петро Петров", AddedBooksCount = 1 }
            };

            _bookRepoMock.Setup(r => r.GetTopActiveUsersWithRecentBooksAsync(It.IsAny<DateTime>(), 5))
                         .ReturnsAsync(mockData);

            // Act
            var result = await _topUsersService.GetTopUsersOfMonthAsync();

            // Assert
            Assert.True(result.IsSuccess);
            var topUsers = result.Value.ToList();

            Assert.Equal(2, topUsers.Count);
            Assert.Equal(1, topUsers[0].UserId);
            Assert.Equal("Іван Іванов", topUsers[0].FullName); // Можемо перевірити й ім'я
            Assert.Equal(3, topUsers[0].AddedBooksCount);
        }

        [Fact]
        public async Task GetTopUsersOfMonthAsync_WhenNoUsers_ShouldReturnEmptyList()
        {
            // Arrange
            var emptyData = new List<TopUserDto>();

            _bookRepoMock.Setup(r => r.GetTopActiveUsersWithRecentBooksAsync(It.IsAny<DateTime>(), 5))
                         .ReturnsAsync(emptyData);

            // Act
            var result = await _topUsersService.GetTopUsersOfMonthAsync();

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value);
        }
    }
}