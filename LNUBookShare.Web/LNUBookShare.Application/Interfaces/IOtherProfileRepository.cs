using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IOtherProfileRepository
    {
        Task<User?> GetUserById(int userId);
        Task<IEnumerable<Book>> GetUserBooks(int userId, string sortBy = "title", string statusFilter = "all");
    }
}
