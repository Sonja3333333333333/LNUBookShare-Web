using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IReviewService
    {
        Task<Result> AddReviewAsync(int bookId, int userId, int rating, string? comment);
        Task<Result<double>> CalculateAverageRatingAsync(int bookId);
        Task<Result<IEnumerable<BookReview>>> GetBookReviewsAsync(int bookId);
        Task<Result<bool>> HasUserReviewedAsync(int bookId, int userId);
    }
}