using System.Threading.Tasks;
using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class ProfileService : IProfileService
    {
        private readonly IProfileRepository _profileRepository;
        private readonly ILogger<ProfileService> _logger;

        public ProfileService(IProfileRepository profileRepository, ILogger<ProfileService> logger)
        {
            _profileRepository = profileRepository;
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
    }
}