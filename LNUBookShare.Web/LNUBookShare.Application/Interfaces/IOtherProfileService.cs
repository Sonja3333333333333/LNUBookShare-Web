using System.Threading.Tasks;
using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IOtherProfileService
    {
        Task<Result<User?>> GetOtherUser(int userId);
        Task<Result<IEnumerable<Book>>> GetOtherUserBooks(int userId, string sortBy = "title", string statusFilter = "all");
    }
}
