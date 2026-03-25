using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LNUBookShare.UnitTests.FavoriteServiceTests;

public class FavoriteServiceClearTests
{
    private readonly Mock<IFavoriteRepository> _favoriteRepoMock;
    private readonly Mock<IBookRepository> _bookRepoMock;
    private readonly Mock<ILogger<FavoriteService>> _loggerMock;
    private readonly FavoriteService _service;

    public FavoriteServiceClearTests()
    {
        _favoriteRepoMock = new Mock<IFavoriteRepository>();
        _bookRepoMock = new Mock<IBookRepository>();
        _loggerMock = new Mock<ILogger<FavoriteService>>();

        _service = new FavoriteService(
            _favoriteRepoMock.Object,
            _bookRepoMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task ClearUserFavoritesAsync_WhenListHasBooks_ReturnsSuccess()
    {
        // Arrange
        int userId = 1;

        _favoriteRepoMock
            .Setup(r => r.GetUserFavoriteBookIdsAsync(userId))
            .ReturnsAsync(new List<int> { 1, 2, 3 });

        _favoriteRepoMock
            .Setup(r => r.ClearAllForUserAsync(userId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _service.ClearUserFavoritesAsync(userId);

        // Assert
        Assert.True(result.IsSuccess);

        _favoriteRepoMock.Verify(r => r.ClearAllForUserAsync(userId), Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) =>
                    v.ToString()!.Contains("очистив список уподобань")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task ClearUserFavoritesAsync_WhenListIsEmpty_ReturnsFailure()
    {
        // Arrange
        int userId = 42;

        _favoriteRepoMock
            .Setup(r => r.GetUserFavoriteBookIdsAsync(userId))
            .ReturnsAsync(new List<int>());

        // Act
        var result = await _service.ClearUserFavoritesAsync(userId);

        // Assert
        Assert.True(result.IsFailure);
        Assert.False(string.IsNullOrEmpty(result.Error));

        _favoriteRepoMock.Verify(
            r => r.ClearAllForUserAsync(It.IsAny<int>()),
            Times.Never);
    }
}