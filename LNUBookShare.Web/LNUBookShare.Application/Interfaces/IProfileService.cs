using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Models;

namespace LNUBookShare.Application.Interfaces
{
    public interface IProfileService
    {
        Task<Result<UserProfileDto>> GetUserProfileAsync(int userId);

        Task<Result<bool>> UpdateProfileAsync(int userId, string firstName, string lastName, int facultyId, string? newAvatarPath);
    }
}