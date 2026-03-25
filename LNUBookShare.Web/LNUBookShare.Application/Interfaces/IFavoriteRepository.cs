using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IFavoriteRepository
    {
        Task<bool> ExistsAsync(int userId, int bookId);
        Task AddAsync(Favorite favorite);
        Task RemoveAsync(int userId, int bookId);
        Task<IEnumerable<int>> GetUserFavoriteBookIdsAsync(int userId);
        Task<IEnumerable<Book>> GetUserFavoriteBooksAsync(int userId);
        Task ClearAllForUserAsync(int userId);
    }
}
