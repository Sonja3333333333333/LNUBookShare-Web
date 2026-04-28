using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Models;

namespace LNUBookShare.Application.Interfaces
{
    public interface ITopUsersService
    {
        Task<Result<IEnumerable<TopUserDto>>> GetTopUsersOfMonthAsync();
    }
}