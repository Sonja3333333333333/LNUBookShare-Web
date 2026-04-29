using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IAdminReviewRepository
    {
        Task<IEnumerable<BookReview>> GetAllWithDetailsAsync();
        Task<BookReview?> GetByIdAsync(int id);
        Task DeleteAsync(BookReview review);
    }
}