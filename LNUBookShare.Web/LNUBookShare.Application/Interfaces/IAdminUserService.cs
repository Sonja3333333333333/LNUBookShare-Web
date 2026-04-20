using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Models;

public interface IAdminUserService
{
    Task<Result<IEnumerable<UserDto>>> GetAllUsersAsync(string? searchTerm = null);
}