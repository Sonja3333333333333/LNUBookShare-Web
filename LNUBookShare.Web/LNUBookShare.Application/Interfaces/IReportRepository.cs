using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IReportRepository
    {
        Task AddAsync(Report report);
        Task<bool> ExistsAsync(int senderId, int reportedUserId);
        Task<IEnumerable<Report>> GetAllWithUsersAsync(); // Щоб адмін бачив імена
        Task<Report?> GetByIdAsync(int id);
    }
}