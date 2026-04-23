using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class AdminBookService : IAdminBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<AdminBookService> _logger;

        public AdminBookService(IBookRepository bookRepository, ILogger<AdminBookService> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<AdminBookDto>>> GetAllBooksAsync()
        {
            _logger.LogInformation("Адміністратор запитує список усіх оголошень (книг).");

            var books = await _bookRepository.GetAllBooksWithOwnersAsync();

            if (books == null)
            {
                _logger.LogWarning("Список книг повернув null.");
                return Result<IEnumerable<AdminBookDto>>.Failure("Дані про книги відсутні.");
            }

            var dtos = MapToAdminBookDto(books);

            _logger.LogInformation("Успішно отримано {Count} оголошень.", books.Count());
            return Result<IEnumerable<AdminBookDto>>.Success(dtos);
        }

        public async Task<Result<IEnumerable<AdminBookDto>>> AdminSearchBooksAsync(string searchBy, string query)
        {
            _logger.LogInformation("Адмін-пошук за критерієм: {SearchBy}, запит: {Query}", searchBy, query);

            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Спроба пошуку з порожнім запитом.");
                return Result<IEnumerable<AdminBookDto>>.Failure("Пошуковий запит не може бути порожнім.");
            }

            var trimmedQuery = query.Trim();
            var books = await _bookRepository.SearchBooksByCriteriaAsync(searchBy, trimmedQuery);

            var dtos = MapToAdminBookDto(books);

            _logger.LogInformation("Пошук завершено. Знайдено: {Count}", dtos.Count());
            return Result<IEnumerable<AdminBookDto>>.Success(dtos);
        }

        private IEnumerable<AdminBookDto> MapToAdminBookDto(IEnumerable<Book> books)
        {
            return books.Select(b => new AdminBookDto
            {
                Id = b.BookId,
                Title = b.Title ?? "Без назви",
                Author = b.Author ?? "Невідомий автор",
                Isbn = b.Isbn ?? "Невідомо",
                OwnerName = b.Owner != null ? $"{b.Owner.FirstName} {b.Owner.LastName}" : "Невідомо",
                OwnerEmail = b.Owner?.Email ?? "Невідомо",
                Status = b.Status,
            }).ToList();
        }
        public async Task<Result> DeleteBookAsync(int bookId)
        {
            _logger.LogInformation("Адміністратор намагається видалити книгу з ID: {BookId}", bookId);

            try
            {
                var book = await _bookRepository.GetByIdAsync(bookId);

                if (book is null)
                {
                    _logger.LogWarning("Книгу з ID {BookId} не знайдено.", bookId);
                    return Result.Failure("Книгу не знайдено.");
                }

                await _bookRepository.DeleteAsync(book);

                _logger.LogInformation("Книгу з ID {BookId} успішно видалено адміністратором.", bookId);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при видаленні книги з ID {BookId}.", bookId);
                return Result.Failure("Помилка при видаленні книги.");
            }
        }
    }
}