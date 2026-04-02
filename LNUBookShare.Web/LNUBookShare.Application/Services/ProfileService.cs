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
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                FacultyName = user.Faculty?.FacultyName ?? string.Empty,
                AvatarPath = user.Avatar?.ImagePath,
            };

            return Result<UserProfileDto>.Success(dto);
        }

        public async Task<Result<bool>> UpdateProfileAsync(
            int userId,
            string firstName,
            string lastName,
            int facultyId,
            string? newAvatarPath)
        {
            var user = await _profileRepository.GetUserDetailsAsync(userId);

            if (user == null)
            {
                _logger.LogWarning("Спроба редагування профілю для неіснуючого користувача з ID: {UserId}", userId);
                return Result<bool>.Failure("Користувача не знайдено.");
            }

            user.FirstName = firstName;
            user.LastName = lastName;
            user.FacultyId = facultyId;

            if (!string.IsNullOrEmpty(newAvatarPath))
            {
                if (user.Avatar == null)
                {
                    user.Avatar = new Image
                    {
                        ImagePath = newAvatarPath,
                        ImageType = "avatar",
                        UploadedAt = DateTime.UtcNow,
                    };
                }
                else
                {
                    user.Avatar.ImagePath = newAvatarPath;
                }
            }

            await _profileRepository.UpdateUserAsync(user);

            _logger.LogInformation("Профіль користувача {UserId} успішно оновлено.", userId);
            return Result<bool>.Success(true);
        }
    }
}