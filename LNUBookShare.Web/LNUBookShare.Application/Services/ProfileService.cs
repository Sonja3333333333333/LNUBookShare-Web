using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
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

            return dto;
        }

        public async Task<Result> AddBookToProfileAsync(Book book)
        {
            if (book == null)
            {
                return Result.Failure("Дані про книгу відсутні.");
            }

            book.Status = "available";
            book.CreatedAt = DateTime.UtcNow;

            await _bookRepository.AddAsync(book);

            _logger.LogInformation("Користувач {UserId} успішно додав книгу {Title}", book.OwnerId, book.Title);
            return Result.Success();
        }

        public async Task<Result<List<Book>>> GetUserBooksAsync(int userId)
        {
            var allBooks = await _bookRepository.GetAllAsync();

            var userBooks = allBooks
                .Where(b => b.OwnerId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();

            return userBooks;
        }

        public async Task<Result> UpdateBookAsync(Book book)
        {
            if (book == null)
            {
                return Result.Failure("Дані для оновлення відсутні.");
            }

            await _bookRepository.UpdateAsync(book);

            _logger.LogInformation("Книгу з ID {BookId} оновлено", book.BookId);
            return Result.Success();
        }

        public async Task<Result> DeleteBookAsync(int bookId)
        {
            var book = await _bookRepository.GetByIdAsync(bookId);
            if (book == null)
            {
                return Result.Failure("Книгу не знайдено.");
            }

            await _bookRepository.DeleteAsync(book);

            _logger.LogInformation("Книгу з ID {BookId} видалено", bookId);
            return Result.Success();
        }

        public async Task<Result> UpdateProfileAsync(int userId, string firstName, string lastName, int facultyId, string? avatarPath)
        {
            var user = await _profileRepository.GetUserByIdAsync(userId);

            if (user == null)
            {
                return Result.Failure("Користувача не знайдено.");
            }

            user.FirstName = firstName;
            user.LastName = lastName;
            user.FacultyId = facultyId;

            if (!string.IsNullOrEmpty(avatarPath))
            {
                user.Avatar = new Image
                {
                    ImagePath = avatarPath,
                    ImageType = "Avatar",
                };
            }

            await _profileRepository.UpdateUserAsync(user);

            return Result.Success();
        }
    }
}