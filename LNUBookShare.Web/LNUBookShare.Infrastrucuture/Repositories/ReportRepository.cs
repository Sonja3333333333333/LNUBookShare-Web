using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

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

        public async Task DeleteAsync(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report != null)
            {
                _context.Reports.Remove(report);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateStatusAsync(int reportId, ReportStatus newStatus)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report != null)
            {
                report.Status = newStatus;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<UserReport>> GetFilteredReportsAsync(string searchBy, string query, string sortBy, string statusFilter)
        {
            var dbQuery = GetSearchQuery(searchBy, query);
            var finalQuery = ApplyFilteringAndSorting(dbQuery, sortBy, statusFilter);

            return await finalQuery.ToListAsync();
        }

        private IQueryable<UserReport> GetReportsBaseQuery()
        {
            return _context.Reports
                .Include(r => r.Sender)
                .Include(r => r.ReportedUser);
        }

        private IQueryable<UserReport> GetSearchQuery(string searchBy, string query)
        {
            var baseQuery = GetReportsBaseQuery();

            if (string.IsNullOrWhiteSpace(query))
            {
                return baseQuery;
            }

            var lowerQuery = query.ToLower();

            return searchBy?.ToLower() switch
            {
                "sender" => baseQuery.Where(r => r.Sender != null &&
                    (EF.Functions.ILike(r.Sender.FirstName, $"%{lowerQuery}%") ||
                     EF.Functions.ILike(r.Sender.LastName, $"%{lowerQuery}%"))),
                "reported" => baseQuery.Where(r => r.ReportedUser != null &&
                    (EF.Functions.ILike(r.ReportedUser.FirstName, $"%{lowerQuery}%") ||
                     EF.Functions.ILike(r.ReportedUser.LastName, $"%{lowerQuery}%"))),
                "details" => baseQuery.Where(r => EF.Functions.ILike(r.Details, $"%{lowerQuery}%")),
                _ => baseQuery
            };
        }

        private IQueryable<UserReport> ApplyFilteringAndSorting(IQueryable<UserReport> query, string sortBy, string statusFilter)
        {
            query = statusFilter?.ToLower() switch
            {
                "active" => query.Where(r => r.Status == ReportStatus.Pending),
                "resolved" => query.Where(r => r.Status == ReportStatus.Resolved),
                "dismissed" => query.Where(r => r.Status == ReportStatus.Dismissed),
                "all" => query,
                _ => query
            };

            query = sortBy switch
            {
                "date" => query.OrderByDescending(r => r.CreatedAt),
                _ => query
            };

            return query;
        }
    }
}