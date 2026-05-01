using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IAdminReviewRepository
    {
        Task<IEnumerable<BookReview>> GetAllWithDetailsAsync(string? searchBy = null, string? query = null, int? ratingFilter = null);
        Task<BookReview?> GetByIdAsync(int id);
        Task DeleteAsync(BookReview review);
    }
}