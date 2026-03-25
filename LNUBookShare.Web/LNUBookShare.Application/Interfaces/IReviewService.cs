using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces;

public interface IReviewService
{
    // Метод повертає твій клас Result (успіх або помилка валідації)
    Task<Result> AddReviewAsync(int bookId, int userId, int rating, string? comment);

    // Рахує середній бал для книги
    Task<double> CalculateAverageRatingAsync(int bookId);

    // Повертає список відгуків для виводу на сторінці
    Task<IEnumerable<BookReview>> GetBookReviewsAsync(int bookId);
}