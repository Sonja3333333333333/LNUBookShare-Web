using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LNUBookShare.Application.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IReviewRepository _reviewRepo;
        private readonly ILogger<ReviewService> _logger;
        private readonly ReviewSettings _settings;

        public ReviewService(IReviewRepository reviewRepo, ILogger<ReviewService> logger, IOptions<ReviewSettings> options)
        {
            _reviewRepo = reviewRepo;
            _logger = logger;
            _settings = options.Value;
        }

        public async Task<Result> AddReviewAsync(int bookId, int userId, int rating, string? comment)
        {
            _logger.LogInformation("Спроба додати відгук від {UserId} для книги {BookId}", userId, bookId);

            if (rating < _settings.MinRating || rating > _settings.MaxRating)
            {
                return Result.Failure($"Неприпустима оцінка. Оберіть від {_settings.MinRating} до {_settings.MaxRating}.");
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                return Result.Failure("Коментар не може бути порожнім.");
            }

            if (comment.Length > _settings.MaxCommentLength)
            {
                _logger.LogWarning("Користувач {UserId} намагався залишити занадто довгий відгук ({Length} символів)", userId, comment.Length);
                return Result.Failure($"Коментар занадто довгий. Максимальна кількість символів: {_settings.MaxCommentLength}.");
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