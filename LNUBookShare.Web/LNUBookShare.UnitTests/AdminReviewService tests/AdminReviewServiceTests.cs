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
        public async Task GetAllReviewsAsync_NoFilter_ReturnsSortedByDateDesc()
        {
            var reviews = new List<BookReview>
            {
                new BookReview
                {
                    ReviewId = 1,
                    Comment = "Стара",
                    CreatedAt = DateTime.UtcNow.AddDays(-5),
                    Reviewer = new User { FirstName = "Іван", LastName = "Мазепа" },
                    Book = new Book { Title = "Кобзар" }
                },
                new BookReview
                {
                    ReviewId = 2,
                    Comment = "Нова",
                    CreatedAt = DateTime.UtcNow,
                    Reviewer = new User { FirstName = "Тарас", LastName = "Франко" },
                    Book = new Book { Title = "Лісова Пісня" }
                }
            };
            _repoMock.Setup(r => r.GetAllWithDetailsAsync()).ReturnsAsync(reviews);

            var result = await _service.GetAllReviewsAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Value.First().ReviewId);
        }

        [Fact]
        public async Task GetAllReviewsAsync_SearchByComment_ReturnsFiltered()
        {
            var reviews = new List<BookReview>
            {
                new BookReview
                {
                    ReviewId = 1, Comment = "спам тут",
                    CreatedAt = DateTime.UtcNow,
                    Reviewer = new User { FirstName = "А", LastName = "Б" },
                    Book = new Book { Title = "Книга" }
                },
                new BookReview
                {
                    ReviewId = 2, Comment = "чудова книга",
                    CreatedAt = DateTime.UtcNow,
                    Reviewer = new User { FirstName = "В", LastName = "Г" },
                    Book = new Book { Title = "Книга 2" }
                }
            };
            _repoMock.Setup(r => r.GetAllWithDetailsAsync()).ReturnsAsync(reviews);

            var result = await _service.GetAllReviewsAsync("comment", "спам");

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            Assert.Equal(1, result.Value.First().ReviewId);
        }

        [Fact]
        public async Task GetAllReviewsAsync_SearchByAuthor_ReturnsFiltered()
        {
            var reviews = new List<BookReview>
            {
                new BookReview
                {
                    ReviewId = 1, Comment = "ок",
                    CreatedAt = DateTime.UtcNow,
                    Reviewer = new User { FirstName = "Іван", LastName = "Мазепа" },
                    Book = new Book { Title = "Книга" }
                },
                new BookReview
                {
                    ReviewId = 2, Comment = "добре",
                    CreatedAt = DateTime.UtcNow,
                    Reviewer = new User { FirstName = "Петро", LastName = "Сагайдачний" },
                    Book = new Book { Title = "Книга 2" }
                }
            };
            _repoMock.Setup(r => r.GetAllWithDetailsAsync()).ReturnsAsync(reviews);

            var result = await _service.GetAllReviewsAsync("author", "Мазепа");

            Assert.True(result.IsSuccess);
            Assert.Single(result.Value);
            Assert.Equal(1, result.Value.First().ReviewId);
        }

        [Fact]
        public async Task DeleteReviewAsync_WhenReviewNotFound_ReturnsFailure()
        {
            _repoMock.Setup(r => r.GetByIdAsync(99))
                .ReturnsAsync((BookReview?)null);

            var result = await _service.DeleteReviewAsync(99);

            Assert.True(result.IsFailure);
            Assert.Equal("Коментар не знайдено.", result.Error);
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