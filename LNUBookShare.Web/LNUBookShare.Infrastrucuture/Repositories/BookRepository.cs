using CloudinaryDotNet;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using LNUBookShare.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Infrastructure.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly AppDbContext _context;

        public BookRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllAsync()
        {
            return await _context.Books.Include(b => b.Owner).Include(b => b.Cover).Include(b => b.ReservationQueues)
            .ThenInclude(rq => rq.User).ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task<Book?> GetByIdMoreDetailsAsync(int id)
        {
            return await _context.Books
                .Include(b => b.Owner)
                .Include(b => b.Cover)
                .Include(b => b.Category)
                .FirstOrDefaultAsync(b => b.BookId == id);
        }

        public async Task<IEnumerable<Book>> GetAllBooksWithOwnersAsync()
        {
            return await _context.Books
                .Include(b => b.Owner)
                .ThenInclude(o => o.Avatar)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task AddAsync(Book book)
        {
            await _context.Books.AddAsync(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Book book)
        {
            _context.Books.Remove(book);
            await _context.SaveChangesAsync();
        }

        public async Task ClearAllAsync()
        {
            await _context.Books.ExecuteDeleteAsync();
        }

        public async Task<IEnumerable<Book>> SearchBooksByCriteriaAsync(string searchBy, string query)
        {
            var dbQuery = GetSearchQuery(searchBy, query);
            return await dbQuery.ToListAsync();
        }

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchBy, string query, string sortBy = "title", string statusFilter = "all")
        {
            var dbQuery = GetSearchQuery(searchBy, query);
            var finalQuery = ApplySortingAndFiltering(dbQuery, sortBy, statusFilter);
            return await finalQuery.ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetRecommendationsAsync(int facultyId, int currentUserId, string sortBy = "title", string statusFilter = "all")
        {
            var dbQuery = _context.Books
                .Include(b => b.Owner)
                .Include(b => b.Cover)
                .Where(b => b.Owner.FacultyId == facultyId && b.OwnerId != currentUserId);

            return await ApplySortingAndFiltering(dbQuery, sortBy, statusFilter).ToListAsync();
        }

        public async Task<IEnumerable<TopUserDto>> GetTopActiveUsersWithRecentBooksAsync(System.DateTime sinceDate, int takeCount)
        {
            var topUsers = await _context.Users
                .Where(u => u.IsActive)
                .Select(u => new
                {
                    User = u,
                    RecentBooksCount = u.Books.Count(b => b.CreatedAt >= sinceDate),
                })
                .Where(x => x.RecentBooksCount > 0)
                .OrderByDescending(x => x.RecentBooksCount)
                .Take(takeCount)
                .Select(x => new TopUserDto
                {
                    UserId = x.User.Id,
                    FullName = x.User.FirstName + " " + x.User.LastName,
                    AvatarId = x.User.AvatarId,
                    AvatarPath = x.User.Avatar != null ? x.User.Avatar.ImagePath : null,
                    AddedBooksCount = x.RecentBooksCount,
                })
                .ToListAsync();

            return topUsers;
        }

        private IQueryable<Book> GetBooksBaseQuery()
        {
            return _context.Books
                .Include(b => b.Owner)
                .Include(b => b.Cover);
        }

        private IQueryable<Book> GetSearchQuery(string searchBy, string query)
        {
            var baseQuery = GetBooksBaseQuery();

            if (string.IsNullOrWhiteSpace(query))
            {
                return Enumerable.Empty<Book>().AsQueryable();
            }

            var lowerQuery = query.ToLower();

            return searchBy?.ToLower() switch
            {
                "author" => baseQuery.Where(b => EF.Functions.ILike(b.Author, $"%{lowerQuery}%")),
                "title" => baseQuery.Where(b => EF.Functions.ILike(b.Title, $"%{lowerQuery}%")),
                "isbn" => baseQuery.Where(b => b.Isbn != null && b.Isbn.Contains(lowerQuery)),
                "owner" => baseQuery.Where(b => b.Owner != null &&
                    (EF.Functions.ILike(b.Owner.FirstName, $"%{lowerQuery}%") ||
                     EF.Functions.ILike(b.Owner.LastName, $"%{lowerQuery}%"))),
                _ => baseQuery
            };
        }

        private IQueryable<Book> ApplySortingAndFiltering(IQueryable<Book> query, string sortBy, string statusFilter)
        {
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

            return query;
        }
    }
}