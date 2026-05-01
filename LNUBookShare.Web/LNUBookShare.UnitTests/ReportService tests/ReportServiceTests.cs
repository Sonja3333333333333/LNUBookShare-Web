using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Moq;
using Xunit;

namespace LNUBookShare.Tests.Services;

public class ReportServiceTests
{
    private readonly Mock<IReportRepository> _reportRepoMock;
    private readonly ReportService _reportService;

    public ReportServiceTests()
    {
        // Створюємо "іграшковий" репозиторій
        _reportRepoMock = new Mock<IReportRepository>();

        // Передаємо його в наш сервіс
        _reportService = new ReportService(_reportRepoMock.Object);
    }

    [Fact]
    public async Task CreateReportAsync_ShouldReturnFailure_WhenSenderIsReportedUser()
    {
        // Arrange
        int userId = 1;

        // Act
        var result = await _reportService.CreateReportAsync(userId, userId, ReportReason.Spam, "Details");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Користувач не може поскаржитися сам на себе.", result.Error);
    }

    [Fact]
    public async Task CreateReportAsync_ShouldReturnFailure_WhenReportAlreadyExists()
    {
        // Arrange
        int senderId = 1;
        int reportedId = 2;

        
        _reportRepoMock.Setup(r => r.ExistsAsync(senderId, reportedId))
                       .ReturnsAsync(true);

        // Act
        var result = await _reportService.CreateReportAsync(senderId, reportedId, ReportReason.Spam, "Details");

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal("Ви вже надсилали скаргу на цього користувача.", result.Error);
    }

    [Fact]
    public async Task CreateReportAsync_ShouldReturnSuccess_WhenDataIsValid()
    {
        // Arrange
        int senderId = 1;
        int reportedId = 2;

        _reportRepoMock.Setup(r => r.ExistsAsync(senderId, reportedId))
                       .ReturnsAsync(false);

        // Act
        var result = await _reportService.CreateReportAsync(senderId, reportedId, ReportReason.Other, "Valid details");

        // Assert
        Assert.True(result.IsSuccess);

        _reportRepoMock.Verify(r => r.AddAsync(It.IsAny<UserReport>()), Times.Once);
    }
}