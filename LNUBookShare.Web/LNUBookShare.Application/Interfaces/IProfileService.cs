using System.Threading.Tasks;
using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Models;

namespace LNUBookShare.Application.Interfaces
{
    public interface IProfileService
    {
        Task<Result<UserProfileDto>> GetUserProfileAsync(int userId);
    }
}