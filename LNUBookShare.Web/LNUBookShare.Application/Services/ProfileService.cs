using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

            return Result<UserProfileDto>.Success(dto);
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

        public async Task<List<Book>> GetUserBooksAsync(int userId)
        {
            var allBooks = await _bookRepository.GetAllAsync();

            return allBooks
                .Where(b => b.OwnerId == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToList();
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
    }
}