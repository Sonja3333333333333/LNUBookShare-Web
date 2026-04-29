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
            string? searchBy = null, string? query = null)
        {
            _logger.LogInformation(
                "Адміністратор запитує список коментарів. SearchBy: {SearchBy}, Query: {Query}",
                searchBy, query);

            var reviews = await _reviewRepository.GetAllWithDetailsAsync();

            if (!string.IsNullOrWhiteSpace(query) && !string.IsNullOrWhiteSpace(searchBy))
            {
                var trimmed = query.Trim().ToLower();
                reviews = searchBy.ToLower() switch
                {
                    "author" => reviews.Where(r =>
                        r.Reviewer != null &&
                        ($"{r.Reviewer.FirstName} {r.Reviewer.LastName}")
                            .ToLower().Contains(trimmed)),
                    "comment" => reviews.Where(r =>
                        r.Comment != null &&
                        r.Comment.ToLower().Contains(trimmed)),
                    _ => reviews
                };
            }

            reviews = reviews.OrderByDescending(r => r.CreatedAt);

            return Result<IEnumerable<BookReview>>.Success(reviews);
        }

        public async Task<Result> DeleteReviewAsync(int reviewId)
        {
            _logger.LogInformation(
                "Адміністратор намагається видалити коментар з ID: {ReviewId}", reviewId);

            var review = await _reviewRepository.GetByIdAsync(reviewId);
            if (review is null)
            {
                _logger.LogWarning("Коментар з ID {ReviewId} не знайдено.", reviewId);
                return Result.Failure("Коментар не знайдено.");
            }

            try
            {
                await _reviewRepository.DeleteAsync(review);
                _logger.LogInformation(
                    "Коментар з ID {ReviewId} успішно видалено.", reviewId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при видаленні коментаря з ID {ReviewId}.", reviewId);
                return Result.Failure("Помилка при видаленні коментаря.");
            }
        }
    }
}