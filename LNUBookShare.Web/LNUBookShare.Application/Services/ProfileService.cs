using System;
using System.Threading.Tasks;
using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(IBookRepository bookRepository, ILogger<ProfileService> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public async Task<Result> AddBookToProfileAsync(Book book)
        {
            // Перевіряємо на валідність
            if (book == null)
            {
                _logger.LogWarning("Спроба додати порожню книгу.");
                return Result.Failure("Дані про книгу відсутні.");
            }

            // Задаємо системні поля (статус за замовчуванням і час створення)
            book.Status = "Доступно"; // Або "Available", залежно від того, як у вас в БД
            book.CreatedAt = DateTime.UtcNow;

            // Зберігаємо через репозиторій
            await _bookRepository.AddAsync(book);

            _logger.LogInformation("Користувач {UserId} успішно додав книгу {Title}", book.OwnerId, book.Title);

            return Result.Success();
        }
    }
}