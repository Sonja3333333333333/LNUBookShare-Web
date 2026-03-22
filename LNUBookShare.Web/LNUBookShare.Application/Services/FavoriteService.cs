using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class FavoriteService : IFavoriteService
    {
        private readonly IFavoriteRepository _favoriteRepository;
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<FavoriteService> _logger;

        public FavoriteService(IFavoriteRepository favoriteRepository, IBookRepository bookRepository, ILogger<FavoriteService> logger)
        {
            _favoriteRepository = favoriteRepository;
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public async Task<Result> ToggleFavoriteAsync(int userId, int bookId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                _logger.LogWarning("Користувач {UserId} спробував вподобати неіснуючу книгу {BookId}", userId, bookId);
                return Result.Failure("Книгу не знайдено.");
            }

            var exists = await _favoriteRepository.ExistsAsync(userId, bookId);

            if (exists)
            {
                await _favoriteRepository.RemoveAsync(userId, bookId);
                _logger.LogInformation("Користувач {UserId} видалив книгу {BookId} з уподобань", userId, bookId);
                return Result.Success();
            }
            else
            {
                var favorite = new Favorite
                {
                    UserId = userId,
                    BookId = bookId,
                    CreatedAt = DateTime.UtcNow,
                };

                await _favoriteRepository.AddAsync(favorite);
                _logger.LogInformation("Користувач {UserId} додав книгу {BookId} до уподобань", userId, bookId);
                return Result.Success();
            }
        }

        public async Task<Result<IEnumerable<int>>> GetUserFavoriteBookIdsAsync(int userId)
        {
            var ids = await _favoriteRepository.GetUserFavoriteBookIdsAsync(userId);
            return Result<IEnumerable<int>>.Success(ids);
        }

        public async Task<Result<IEnumerable<Book>>> GetUserFavoriteBooksAsync(int userId)
        {
            var books = await _favoriteRepository.GetUserFavoriteBooksAsync(userId);
            return Result<IEnumerable<Book>>.Success(books);
        }
    }
}