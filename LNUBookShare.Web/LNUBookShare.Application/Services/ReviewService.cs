using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services;

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
            return Result.Failure("Неприпустима оцінка");
        }

        if (string.IsNullOrWhiteSpace(comment))
        {
            return Result.Failure("Коментар не може бути порожнім");
        }

        var review = new BookReview
        {
            BookId = bookId,
            ReviewerId = userId,
            Rating = rating,
            Comment = comment,
            CreatedAt = DateTime.UtcNow,
        };

        await _reviewRepo.AddAsync(review);
        return Result.Success();
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
}