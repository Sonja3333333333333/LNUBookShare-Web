using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly ILogger<ReviewService> _logger;

        public ReviewService(IReviewRepository reviewRepo, ILogger<ReviewService> logger)
        {
            _reviewRepo = reviewRepo;
            _logger = logger;
        }

        public async Task<Result> AddReviewAsync(int bookId, int userId, int rating, string? comment)
        {
            _logger.LogInformation("Спроба додати відгук від {UserId} для книги {BookId}", userId, bookId);

            if (rating < 1 || rating > 5)
            {
                return Result.Failure("Неприпустима оцінка. Оберіть від 1 до 5.");
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                return Result.Failure("Коментар не може бути порожнім.");
            }

            if (await _reviewRepo.ExistsAsync(bookId, userId))
            {
                _logger.LogWarning("Користувач {UserId} намагався повторно оцінити книгу {BookId}", userId, bookId);
                return Result.Failure("Ви вже залишили відгук до цієї книги. Один користувач — один відгук.");
            }

            var review = new BookReview
            {
                BookId = bookId,
                ReviewerId = userId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow,
            };

            try
            {
                await _reviewRepo.AddAsync(review);
                _logger.LogInformation("Відгук від {UserId} успішно додано для книги {BookId}", userId, bookId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при збереженні відгуку в базу для книги {BookId}", bookId);
                return Result.Failure("Сталася помилка при збереженні відгуку.");
            }
        }

        public async Task<Result<double>> CalculateAverageRatingAsync(int bookId)
        {
            var reviews = await _reviewRepo.GetByBookIdAsync(bookId);
            var average = reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 0.0;

            return average;
        }

        public async Task<Result<IEnumerable<BookReview>>> GetBookReviewsAsync(int bookId)
        {
            var reviews = await _reviewRepo.GetByBookIdAsync(bookId);

            return Result<IEnumerable<BookReview>>.Success(reviews);
        }

        public async Task<Result<bool>> HasUserReviewedAsync(int bookId, int userId)
        {
            return await _reviewRepo.ExistsAsync(bookId, userId); // Неявне перетворення спрацює
        }
    }
}