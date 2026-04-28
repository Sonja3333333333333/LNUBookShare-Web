using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IReportRepository
    {
        Task AddAsync(UserReport report);
        Task<bool> ExistsAsync(int senderId, int reportedUserId);
        Task<IEnumerable<UserReport>> GetAllWithUsersAsync(); // Щоб адмін бачив імена
        Task<UserReport?> GetByIdAsync(int id);
    }
}