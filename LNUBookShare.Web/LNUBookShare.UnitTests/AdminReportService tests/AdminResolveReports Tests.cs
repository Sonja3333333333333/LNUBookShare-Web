using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.AdminReportService_tests
{
    public class AdminResolveReports_Tests
    {
        private readonly Mock<IReportRepository> _reportRepoMock;
        private readonly Mock<ILogger<AdminReportService>> _loggerMock;
        private readonly AdminReportService _service;

        public AdminResolveReports_Tests()
        {
            _reportRepoMock = new Mock<IReportRepository>();
            _loggerMock = new Mock<ILogger<AdminReportService>>();
            _service = new AdminReportService(_reportRepoMock.Object, _loggerMock.Object);
        }


        [Fact]
        public async Task ResolveReportAsync_ShouldReturnSuccess_WhenReportIsPending()
        {
            // Arrange
            var reportId = 1;
            var report = new UserReport { Id = reportId, Status = ReportStatus.Pending };
            _reportRepoMock.Setup(r => r.GetByIdAsync(reportId)).ReturnsAsync(report);

            // Act
            var result = await _service.ResolveReportAsync(reportId);

            // Assert
            Assert.True(result.IsSuccess);
            _reportRepoMock.Verify(r => r.UpdateStatusAsync(reportId, ReportStatus.Resolved), Times.Once);
        }

        [Fact]
        public async Task ResolveReportAsync_ShouldReturnFailure_WhenReportNotFound()
        {
            // Arrange
            _reportRepoMock.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync((UserReport)null!);

            // Act
            var result = await _service.ResolveReportAsync(999);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Скаргу не знайдено у базі даних.", result.Error);
        }

        [Fact]
        public async Task ResolveReportAsync_ShouldReturnFailure_WhenAlreadyResolved()
        {
            // Arrange
            var report = new UserReport { Id = 1, Status = ReportStatus.Resolved };
            _reportRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(report);

            // Act
            var result = await _service.ResolveReportAsync(1);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Ця скарга вже має статус 'Вирішена'.", result.Error);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task DeleteReportAsync_ShouldReturnFailure_ForInvalidId(int invalidId)
        {
            // Act
            var result = await _service.DeleteReportAsync(invalidId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Некоректний ID.", result.Error);
        }

        [Fact]
        public async Task DeleteReportAsync_ShouldReturnFailure_WhenReportDoesNotExist()
        {
            // Arrange
            _reportRepoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((UserReport)null!);

            // Act
            var result = await _service.DeleteReportAsync(1);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Скаргу не знайдено.", result.Error);
        }

        [Fact]
        public async Task DeleteReportAsync_ShouldReturnSuccess_WhenDeleted()
        {
            // Arrange
            var reportId = 1;
            _reportRepoMock.Setup(r => r.GetByIdAsync(reportId)).ReturnsAsync(new UserReport { Id = reportId });

            // Act
            var result = await _service.DeleteReportAsync(reportId);

            // Assert
            Assert.True(result.IsSuccess);
            _reportRepoMock.Verify(r => r.DeleteAsync(reportId), Times.Once);
        }
    }
}