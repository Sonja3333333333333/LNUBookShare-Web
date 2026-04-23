using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Models;

namespace LNUBookShare.Application.Interfaces
{
    public interface IAdminUserService
    {
        Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync(string? searchTerm = null);
        Task<Result> BlockUserAsync(int userId);
        Task<Result> UnblockUserAsync(int userId);
    }
}