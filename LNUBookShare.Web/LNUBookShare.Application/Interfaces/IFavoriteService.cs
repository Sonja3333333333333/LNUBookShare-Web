using System.Collections.Generic;
using System.Threading.Tasks;
using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;

namespace LNUBookShare.Application.Interfaces
{
    public interface IFavoriteService
    {
        Task<Result> ToggleFavoriteAsync(int userId, int bookId);

        Task<Result<IEnumerable<int>>> GetUserFavoriteBookIdsAsync(int userId);

        Task<Result<IEnumerable<Book>>> GetUserFavoriteBooksAsync(int userId);
        Task<Result<IEnumerable<Book>>> GetUserFavoriteBooksAsync(int userId, FavoriteBooksQueryParameters parameters);
    }
}