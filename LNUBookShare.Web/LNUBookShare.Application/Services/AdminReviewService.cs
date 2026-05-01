using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class AdminReviewService : IAdminReviewService
    {
        private readonly IAdminReviewRepository _reviewRepository;
        private readonly ILogger<AdminReviewService> _logger;

        public AdminReviewService(
            IAdminReviewRepository reviewRepository,
            ILogger<AdminReviewService> logger)
        {
            _reviewRepository = reviewRepository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<BookReview>>> GetAllReviewsAsync(
            string? searchBy = null,
            string? query = null,
            int? ratingFilter = null)
        {
            _logger.LogInformation(
                "Адмін запитує список відгуків. SearchBy={SearchBy}, Query={Query}, Rating={Rating}", searchBy, query, ratingFilter);

            var reviews = await _reviewRepository.GetAllWithDetailsAsync(searchBy, query, ratingFilter);

            _logger.LogInformation("Знайдено {Count} відгуків.", reviews.Count());
            return Result<IEnumerable<BookReview>>.Success(reviews);
        }

        public async Task<Result> DeleteReviewAsync(int reviewId)
        {
            _logger.LogInformation(
                "Адміністратор намагається видалити відгук з ID: {ReviewId}", reviewId);

            var review = await _reviewRepository.GetByIdAsync(reviewId);

            if (review is null)
            {
                _logger.LogWarning("Відгук з ID {ReviewId} не знайдено.", reviewId);
                return Result.Failure("Відгук не знайдено.");
            }

            await _reviewRepository.DeleteAsync(review);

            _logger.LogInformation(
                "Відгук з ID {ReviewId} успішно видалено.", reviewId);
            return Result.Success();
        }
    }
}