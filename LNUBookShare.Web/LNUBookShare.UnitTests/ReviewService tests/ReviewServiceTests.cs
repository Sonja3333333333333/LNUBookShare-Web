using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LNUBookShare.UnitTests.ReviewService_tests
{
    public class ReviewServiceTests
    {
        private readonly Mock<IReviewRepository> _reviewRepoMock;
        private readonly Mock<ILogger<ReviewService>> _loggerMock;
        private readonly ReviewService _reviewService;

        public ReviewServiceTests()
        {
            _reviewRepoMock = new Mock<IReviewRepository>();
            _loggerMock = new Mock<ILogger<ReviewService>>();

            _reviewService = new ReviewService(_reviewRepoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task AddReviewAsync_WithValidData_ShouldReturnSuccess()
        {
            // Arrange
            int bookId = 1, userId = 1, rating = 5;
            string comment = "Чудова книга!";

            // Act
            var result = await _reviewService.AddReviewAsync(bookId, userId, rating, comment);

            // Assert
            Assert.True(result.IsSuccess);
            _reviewRepoMock.Verify(r => r.AddAsync(It.IsAny<BookReview>()), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(6)]
        public async Task AddReviewAsync_WithInvalidRating_ShouldReturnFailure(int invalidRating)
        {
            // Act
            var result = await _reviewService.AddReviewAsync(1, 1, invalidRating, "Тест");

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Неприпустима оцінка", result.Error);
            _reviewRepoMock.Verify(r => r.AddAsync(It.IsAny<BookReview>()), Times.Never);
        }

        [Fact]
        public async Task CalculateAverageRatingAsync_ShouldReturnCorrectAverage()
        {
            // Arrange
            int bookId = 1;
            var reviews = new List<BookReview>
            {
                new BookReview { Rating = 4 },
                new BookReview { Rating = 5 }
            };

            _reviewRepoMock
                .Setup(r => r.GetByBookIdAsync(bookId))
                .ReturnsAsync(reviews);

            // Act
            var result = await _reviewService.CalculateAverageRatingAsync(bookId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(4.5, result.Value); 
        }

        [Fact]
        public async Task CalculateAverageRatingAsync_WhenNoReviews_ShouldReturnZero()
        {
            // Arrange
            _reviewRepoMock
                .Setup(r => r.GetByBookIdAsync(It.IsAny<int>()))
                .ReturnsAsync(new List<BookReview>());

            // Act
            var result = await _reviewService.CalculateAverageRatingAsync(1);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(0.0, result.Value);
        }
    }
}