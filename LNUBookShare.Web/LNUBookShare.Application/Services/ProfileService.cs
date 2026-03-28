using System;
using System.Threading.Tasks;
using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Models;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(
            IProfileRepository profileRepository,
            IBookRepository bookRepository,
            ILogger<ProfileService> logger)
        {
            _profileRepository = profileRepository;
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public async Task<Result<UserProfileDto>> GetUserProfileAsync(int userId)
        {
            var user = await _profileRepository.GetUserDetailsAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("Спроба перегляду профілю для неіснуючого користувача з ID: {UserId}", userId);
                return Result<UserProfileDto>.Failure("Користувача не знайдено.");
            }

            var dto = new UserProfileDto
            {
                UserId = user.Id,
                FirstName = user.FirstName ?? "Не вказано",
                LastName = user.LastName ?? "Не вказано",
                Email = user.Email ?? "Не вказано",
                FacultyName = user.Faculty?.FacultyName ?? "Факультет не вказано",
                AvatarPath = user.Avatar?.ImagePath,
            };

            _logger.LogInformation("Успішно сформовано профіль для користувача з ID: {UserId}", userId);

            return Result<UserProfileDto>.Success(dto);
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