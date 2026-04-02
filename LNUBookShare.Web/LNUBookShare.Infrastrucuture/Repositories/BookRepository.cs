using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
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
            return await _context.Books.Include(b => b.Owner).Include(b => b.Cover).ToListAsync();
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

        public async Task<IEnumerable<Book>> SearchBooksAsync(string searchBy, string query, string sortBy = "title", string statusFilter = "all")
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new List<Book>();
            }

            var lowerQuery = query.ToLower();
            var lowerSearchBy = searchBy.ToLower();

            var dbQuery = _context.Books
                .Include(b => b.Owner)
                .Include(b => b.Cover)
                .AsQueryable();

            if (lowerSearchBy == "author")
            {
                dbQuery = dbQuery.Where(b => EF.Functions.ILike(b.Author, $"%{lowerQuery}%"));
            }
            else if (lowerSearchBy == "title")
            {
                dbQuery = dbQuery.Where(b => EF.Functions.ILike(b.Title, $"%{lowerQuery}%"));
            }
            else
            {
                dbQuery = dbQuery.Where(b => b.Isbn != null && b.Isbn.Contains(lowerQuery));
            }

            return await ApplySortingAndFiltering(dbQuery, sortBy, statusFilter).ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetRecommendationsAsync(int facultyId, int currentUserId, string sortBy = "title", string statusFilter = "all")
        {
            var dbQuery = _context.Books
                .Include(b => b.Owner)
                .Include(b => b.Cover)
                .Where(b => b.Owner.FacultyId == facultyId && b.OwnerId != currentUserId);

            return await ApplySortingAndFiltering(dbQuery, sortBy, statusFilter).ToListAsync();
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