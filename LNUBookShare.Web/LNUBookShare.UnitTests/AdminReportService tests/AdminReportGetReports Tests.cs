using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.AdminReportService_tests
{
    public class AdminReportGetReports_Tests
    {
        private readonly Mock<IReportRepository> _reportRepoMock;
        private readonly Mock<ILogger<AdminReportService>> _loggerMock;
        private readonly AdminReportService _service;

        public AdminReportGetReports_Tests()
        {
            _reportRepoMock = new Mock<IReportRepository>();
            _loggerMock = new Mock<ILogger<AdminReportService>>();
            _service = new AdminReportService(_reportRepoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetReportsAsync_ShouldReturnSuccess_WhenReportsExist()
        {
            // Arrange
            var reports = new List<UserReport> { new UserReport { Id = 1 } };
            _reportRepoMock.Setup(r => r.GetFilteredReportsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(reports);

            // Act
            var result = await _service.GetReportsAsync("some query", "sender", "date", "active");

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetReportsAsync_ShouldHandleEmptyQuery_ByTrimmingToEmptyString()
        {
            // Arrange
            _reportRepoMock.Setup(r => r.GetFilteredReportsAsync(It.IsAny<string>(), string.Empty, It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync(new List<UserReport>());

            // Act
            var result = await _service.GetReportsAsync("   ", "sender", "date", "active");

            // Assert
            _reportRepoMock.Verify(r => r.GetFilteredReportsAsync(It.IsAny<string>(), string.Empty, It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task GetReportsAsync_ShouldReturnFailure_WhenRepositoryReturnsNull()
        {
            // Arrange
            _reportRepoMock.Setup(r => r.GetFilteredReportsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((IEnumerable<UserReport>)null!);

            // Act
            var result = await _service.GetReportsAsync(null);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Дані про скарги відсутні.", result.Error);
        }
    }
}
