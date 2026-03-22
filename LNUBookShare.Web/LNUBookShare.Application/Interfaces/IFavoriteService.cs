using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IFavoriteService
    {
        Task<Result> ToggleFavoriteAsync(int userId, int bookId);
        Task<Result<IEnumerable<int>>> GetUserFavoriteBookIdsAsync(int userId);

        Task<Result<IEnumerable<Book>>> GetUserFavoriteBooksAsync(int userId);
    }
}