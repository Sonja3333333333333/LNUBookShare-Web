using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IAdminReviewService
    {
        Task<Result<IEnumerable<BookReview>>> GetAllReviewsAsync(
            string? searchBy = null, string? query = null);
        Task<Result> DeleteReviewAsync(int reviewId);
    }
}
//12