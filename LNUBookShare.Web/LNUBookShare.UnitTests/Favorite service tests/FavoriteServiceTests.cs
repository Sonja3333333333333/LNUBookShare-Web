using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.Services
{
    public class FavoriteServiceTests
    {
        private readonly Mock<IFavoriteRepository> _favoriteRepositoryMock;
        private readonly Mock<IBookRepository> _bookRepositoryMock;
        private readonly Mock<ILogger<FavoriteService>> _loggerMock;
        private readonly FavoriteService _favoriteService;

        public FavoriteServiceTests()
        {
            _favoriteRepositoryMock = new Mock<IFavoriteRepository>();
            _bookRepositoryMock = new Mock<IBookRepository>();
            _loggerMock = new Mock<ILogger<FavoriteService>>();

            _favoriteService = new FavoriteService(
                _favoriteRepositoryMock.Object,
                _bookRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetUserFavoriteBooksAsync_ValidParameters_ReturnsSuccessResultWithBooks()
        {
            int userId = 1;
            var parameters = new FavoriteBooksQueryParameters
            {
                Status = "available",
                SortBy = "title",
                IsDescending = false
            };

            var expectedBooks = new List<Book>
            {
                new Book { BookId = 1, Title = "Кобзар", Status = "available" },
                new Book { BookId = 2, Title = "Тіні забутих предків", Status = "available" }
            };

            _favoriteRepositoryMock
                .Setup(repo => repo.GetFavoriteBooksAsync(userId, parameters))
                .ReturnsAsync(expectedBooks);

            var result = await _favoriteService.GetUserFavoriteBooksAsync(userId, parameters);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(2, result.Value.Count()); 

            _favoriteRepositoryMock.Verify(repo => repo.GetFavoriteBooksAsync(userId, parameters), Times.Once);
        }

        [Fact]
        public async Task GetUserFavoriteBooksAsync_InvalidSortBy_ReturnsFailureResult()
        {
            int userId = 1;
            var parameters = new FavoriteBooksQueryParameters
            {
                SortBy = "SomeHackedColumn" 
            };

            var result = await _favoriteService.GetUserFavoriteBooksAsync(userId, parameters);

            Assert.False(result.IsSuccess); 
            Assert.Equal("Надано невалідний критерій сортування. Допустимі значення: Author, Title, Year.", result.Error); 

            _favoriteRepositoryMock.Verify(repo => repo.GetFavoriteBooksAsync(It.IsAny<int>(), It.IsAny<FavoriteBooksQueryParameters>()), Times.Never);
        }
    }
}