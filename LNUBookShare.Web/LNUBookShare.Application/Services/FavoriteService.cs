using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
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

        public async Task<Result<IEnumerable<Book>>> GetUserFavoriteBooksAsync(int userId, FavoriteBooksQueryParameters parameters)
        {
            _logger.LogInformation("Користувач {UserId} запросив список вподобань. Параметри - Сортування: {SortBy}, За спаданням: {IsDescending}, Статус: {Status}", userId, parameters.SortBy, parameters.IsDescending, parameters.Status ?? "Не вказано");

            var validSortColumns = new[] { "author", "title", "year", null, string.Empty };
            var normalizedSortBy = parameters.SortBy?.Trim().ToLower();

            if (!validSortColumns.Contains(normalizedSortBy))
            {
                _logger.LogWarning("Користувач {UserId} передав невалідний критерій сортування: {SortBy}", userId, parameters.SortBy);
                return Result<IEnumerable<Book>>.Failure("Надано невалідний критерій сортування. Допустимі значення: Author, Title, Year.");
            }

            var books = await _favoriteRepository.GetFavoriteBooksAsync(userId, parameters);

            return Result<IEnumerable<Book>>.Success(books);
        }
    }
}