using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface INotificationRepository
    {
        Task AddAsync(Notification notification);
        Task<IEnumerable<Notification>> GetByUserIdAsync(int userId);
        Task<Notification?> GetByIdAsync(int id);
        Task UpdateAsync(Notification notification);

        Task DeleteAsync(Notification notification);
        Task<List<Notification>> GetStaleNotificationsAsync(DateTime threshold, CancellationToken ct);
        Task<bool> ExistsTodayAsync(int userId, int? bookId, string messagePart);
        Task<List<int>> GetUserIdsWithPendingNotificationsAsync(DateTime threshold);
    }
}