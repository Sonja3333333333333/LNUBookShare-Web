using System.Threading.Tasks;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IProfileRepository
    {
        Task<User?> GetUserDetailsAsync(int userId);
    }
}