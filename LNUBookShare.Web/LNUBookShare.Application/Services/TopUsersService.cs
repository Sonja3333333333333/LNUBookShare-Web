using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class TopUsersService : ITopUsersService
    {
        private readonly IBookRepository _bookRepo;
        private readonly ILogger<TopUsersService> _logger;

        public TopUsersService(IBookRepository bookRepo, ILogger<TopUsersService> logger)
        {
            _bookRepo = bookRepo;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<TopUserDto>>> GetTopUsersOfMonthAsync()
        {
            try
            {
                var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);

                var topUsers = await _bookRepo.GetTopActiveUsersWithRecentBooksAsync(thirtyDaysAgo, 5);

                _logger.LogInformation("Успішно сформовано рейтинг користувачів місяця. Знайдено: {Count}", topUsers.Count());

                return Result<IEnumerable<TopUserDto>>.Success(topUsers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при формуванні рейтингу користувачів місяця.");
                return Result<IEnumerable<TopUserDto>>.Failure("Помилка при завантаженні рейтингу.");
            }
        }
    }
}