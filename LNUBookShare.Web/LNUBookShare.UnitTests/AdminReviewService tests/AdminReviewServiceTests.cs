using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.AdminReviewService_tests
{
    public class AdminReviewServiceTests
    {
        private readonly Mock<IAdminReviewRepository> _repoMock;
        private readonly Mock<ILogger<AdminReviewService>> _loggerMock;
        private readonly AdminReviewService _service;

        public AdminReviewServiceTests()
        {
            _repoMock = new Mock<IAdminReviewRepository>();
            _loggerMock = new Mock<ILogger<AdminReviewService>>();
            _service = new AdminReviewService(_repoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllReviewsAsync_ShouldPassParametersToRepositoryAndReturnSuccess()
        {
            // Arrange
            var expectedReviews = new List<BookReview> { new BookReview { ReviewId = 1 } };

            _repoMock.Setup(r => r.GetAllWithDetailsAsync(
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<int?>()))
                .ReturnsAsync(expectedReviews);

            // Act
            var result = await _service.GetAllReviewsAsync("comment", "спам", 5);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);

            _repoMock.Verify(r => r.GetAllWithDetailsAsync("comment", "спам", 5), Times.Once);
        }

        [Fact]
        public async Task DeleteReviewAsync_WhenReviewNotFound_ReturnsFailure()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((BookReview?)null);

            var result = await _service.DeleteReviewAsync(99);

            Assert.True(result.IsFailure);
            Assert.Equal("Відгук не знайдено.", result.Error);
            _repoMock.Verify(r => r.DeleteAsync(It.IsAny<BookReview>()), Times.Never);
        }

        [Fact]
        public async Task DeleteReviewAsync_WhenReviewExists_ReturnsSuccess()
        {
            var review = new BookReview { ReviewId = 1, Comment = "test" };
            _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(review);

            var result = await _service.DeleteReviewAsync(1);

            Assert.True(result.IsSuccess);
            _repoMock.Verify(r => r.DeleteAsync(review), Times.Once);
        }
    }
}