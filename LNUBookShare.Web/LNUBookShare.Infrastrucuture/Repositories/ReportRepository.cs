using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Infrastructure.Repositories
{
    public class ReportRepository : IReportRepository
    {
        private readonly AppDbContext _context;

        public ReportRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserReport report)
        {
            await _context.Reports.AddAsync(report);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int senderId, int reportedUserId)
        {
            // Перевіряємо, чи вже є запис у таблиці за нашим унікальним індексом
            return await _context.Reports
                .AnyAsync(r => r.SenderId == senderId && r.ReportedUserId == reportedUserId);
        }

        public async Task<IEnumerable<UserReport>> GetAllWithUsersAsync()
        {
            return await _context.Reports
                .Include(r => r.Sender)
                .Include(r => r.ReportedUser)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<UserReport?> GetByIdAsync(int id)
        {
            return await _context.Reports
                .Include(r => r.Sender)
                .Include(r => r.ReportedUser)
                .FirstOrDefaultAsync(r => r.Id == id);
        }
    }
}