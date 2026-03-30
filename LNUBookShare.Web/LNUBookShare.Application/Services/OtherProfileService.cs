using System.Threading.Tasks;
using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class OtherProfileService : IOtherProfileService
    {
        private readonly IOtherProfileRepository _otherProfileRepository;
        private readonly ILogger<OtherProfileService> _logger;

        public OtherProfileService(IOtherProfileRepository otherProfileRepository, ILogger<OtherProfileService> logger)
        {
            _otherProfileRepository = otherProfileRepository;
            _logger = logger;
        }

        public async Task<Result<User?>> GetOtherUser(int userId)
        {
            var user = await _otherProfileRepository.GetUserById(userId);

            if (user is null)
            {
                _logger.LogWarning("Користувача з ID: {UserId} не існує", userId);
                return Result<User?>.Failure("Користувача не знайдено.");
            }

            _logger.LogInformation("Спроба перегляду профілю користувача з ID: {UserId} успішна!", userId);
            return Result<User?>.Success(user);
        }

        public async Task<Result<IEnumerable<Book>>> GetOtherUserBooks(int userId, string sortBy = "title", string statusFilter = "all")
        {
            var user = await _otherProfileRepository.GetUserById(userId);
            if (user is null)
            {
                _logger.LogWarning("Спроба отримати книги неіснуючого користувача з ID: {UserId}", userId);
                return Result<IEnumerable<Book>>.Failure("Користувача не знайдено.");
            }

            _logger.LogInformation("Книги користувача {UserId} успішно отримані", userId);

            var books = await _otherProfileRepository.GetUserBooks(userId, sortBy, statusFilter);
            return Result<IEnumerable<Book>>.Success(books);
        }
    }
}
