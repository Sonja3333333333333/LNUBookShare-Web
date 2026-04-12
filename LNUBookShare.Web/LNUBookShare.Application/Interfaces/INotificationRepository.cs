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
    }
}