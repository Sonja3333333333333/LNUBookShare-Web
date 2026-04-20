using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class AdminUserService : IAdminUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ILogger<AdminUserService> _logger;

        public AdminUserService(IUserRepository userRepository, ILogger<AdminUserService> logger)
        {
            _userRepository = userRepository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync(string? searchTerm = null)
        {
            _logger.LogInformation("Адміністратор запитує список користувачів. Пошук: {SearchTerm}", searchTerm);

            try
            {
                var users = await _userRepository.GetUsersWithDetailsAsync(searchTerm);

                var dtos = users.Select(u => new UserDto
                {
                    Id = u.Id.ToString(),
                    FullName = $"{u.FirstName} {u.LastName}",
                    Email = u.Email!,
                    FacultyName = u.Faculty != null ? u.Faculty.FacultyName : "Не вказано",
                    EmailConfirmed = u.EmailConfirmed,
                });

                _logger.LogInformation("Успішно отримано {Count} користувачів.", users.Count());
                return Result<IEnumerable<UserDto>>.Success(dtos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при отриманні списку користувачів.");
                return Result<IEnumerable<UserDto>>.Failure("Помилка при завантаженні даних.");
            }
        }
    }
}
