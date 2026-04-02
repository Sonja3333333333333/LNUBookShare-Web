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

            // 1. Валідація вхідних даних
            if (rating < 1 || rating > 5)
            {
                return Result.Failure("Неприпустима оцінка. Оберіть від 1 до 5.");
            }

            if (string.IsNullOrWhiteSpace(comment))
            {
                return Result.Failure("Коментар не може бути порожнім.");
            }

            // 2. ПЕРЕВІРКА НА ДУБЛІКАТ (Ось тут фікс!)
            // Ми питаємо в репозиторія, чи вже є відгук від цього юзера на цю книгу
            if (await _reviewRepo.ExistsAsync(bookId, userId))
            {
                _logger.LogWarning("Користувач {UserId} намагався повторно оцінити книгу {BookId}", userId, bookId);
                return Result.Failure("Ви вже залишили відгук до цієї книги. Один користувач — один відгук.");
            }

            // 3. Якщо все ок — створюємо відгук
            var review = new BookReview
            {
                BookId = bookId,
                ReviewerId = userId, // Використовуємо правильне поле з ентіті
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

        public async Task<double> CalculateAverageRatingAsync(int bookId)
        {
            var reviews = await _reviewRepo.GetByBookIdAsync(bookId);
            return reviews.Any() ? Math.Round(reviews.Average(r => r.Rating), 1) : 0.0;
        }

        public async Task<IEnumerable<BookReview>> GetBookReviewsAsync(int bookId)
        {
            return await _reviewRepo.GetByBookIdAsync(bookId);
        }

        public async Task<bool> HasUserReviewedAsync(int bookId, int userId)
        {
            return await _reviewRepo.ExistsAsync(bookId, userId);
        }
    }
}