using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Infrastructure.Repositories
{
    public class OtherProfileRepository : IOtherProfileRepository
    {
        private readonly AppDbContext _context;

        public OtherProfileRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User?> GetUserById(int userId)
        {
            var user = await _context.Users
                .Include(u => u.Faculty)
                .Include(u => u.Avatar)
                .FirstOrDefaultAsync(u => u.Id == userId);
            return user;
        }

        public async Task<IEnumerable<Book>> GetUserBooks(int userId, string sortBy = "title", string statusFilter = "all")
        {
            var query = _context.Books
                .Include(f => f.Cover)
                .Include(f => f.Owner)
                .Where(f => f.OwnerId == userId);

            if (!string.IsNullOrEmpty(statusFilter) && statusFilter != "all")
            {
                query = query.Where(b => b.Status == statusFilter);
            }

            query = sortBy switch
            {
                "author" => query.OrderBy(b => b.Author),
                "year" => query.OrderByDescending(b => b.Year),
                _ => query.OrderBy(b => b.Title)
            };

            return await query.ToListAsync();
        }
    }
}
