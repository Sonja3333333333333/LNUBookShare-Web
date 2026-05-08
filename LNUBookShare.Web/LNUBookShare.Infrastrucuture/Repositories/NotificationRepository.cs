using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Infrastructure.Repositories // Перевір свій неймспейс!
{
    public class NotificationRepository : INotificationRepository
    {
        private readonly AppDbContext _context;

        public NotificationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Notification notification)
        {
            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Notification>> GetByUserIdAsync(int userId)
        {
            return await _context.Notifications
                .Include(n => n.Book)
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task<Notification?> GetByIdAsync(int id)
        {
            return await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id);
        }

        public async Task UpdateAsync(Notification notification)
        {
            _context.Notifications.Update(notification);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Notification notification)
        {
            _context.Notifications.Remove(notification);
            await _context.SaveChangesAsync();
        }

        public async Task<List<Notification>> GetStaleNotificationsAsync(DateTime threshold, CancellationToken ct)
        {
            return await _context.Notifications
                .Where(n => n.CreatedAt < threshold)
                .ToListAsync(ct);
        }

        public async Task<bool> ExistsTodayAsync(int userId, int? bookId, string messagePart)
        {
            var today = DateTime.UtcNow.Date;

            return await _context.Notifications
                .AnyAsync(n => n.UserId == userId
                            && n.BookId == bookId
                            && n.Message.Contains(messagePart)
                            && n.CreatedAt.Date == today);
        }

        public async Task<List<int>> GetUserIdsWithPendingNotificationsAsync(DateTime threshold)
        {
            // Тепер цей метод реально фільтрує по часу
            return await _context.Notifications
               .Where(n => n.CreatedAt < threshold)
               .Select(n => n.UserId)
               .Distinct()
               .ToListAsync();
        }
    }
}