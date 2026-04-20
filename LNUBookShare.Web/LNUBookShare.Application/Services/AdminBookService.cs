using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
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

            try
            {
                var books = await _bookRepository.GetAllBooksWithOwnersAsync();

                var dtos = books.Select(b => new AdminBookDto
                {
                    Id = b.BookId,
                    Title = b.Title ?? "Без назви",
                    Author = b.Author ?? "Невідомий автор",
                    OwnerName = b.Owner != null ? $"{b.Owner.FirstName} {b.Owner.LastName}" : "Невідомо",
                    OwnerEmail = b.Owner != null ? b.Owner.Email! : "Невідомо",
                    Status = b.Status,
                });

                _logger.LogInformation("Успішно отримано {Count} оголошень.", books.Count());
                return Result<IEnumerable<AdminBookDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при отриманні списку оголошень.");
                return Result<IEnumerable<AdminBookDto>>.Failure("Помилка при завантаженні бази книг.");
            }
        }
    }
}