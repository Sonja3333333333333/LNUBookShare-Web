using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Infrastructure.Repositories
{
    public class AdminReviewRepository : IAdminReviewRepository
    {
        private readonly AppDbContext _context;

        public AdminReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<BookReview>> GetAllWithDetailsAsync(string? searchBy = null, string? query = null, int? ratingFilter = null)
        {
            var dbQuery = _context.BookReviews
                .Include(r => r.Reviewer)
                .Include(r => r.Book)
                .AsQueryable();

            if (ratingFilter.HasValue)
            {
                dbQuery = dbQuery.Where(r => r.Rating == ratingFilter.Value);
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                var lowerQuery = query.Trim().ToLower();
                dbQuery = searchBy?.ToLower() switch
                {
                    "comment" => dbQuery.Where(r => r.Comment != null && r.Comment.ToLower().Contains(lowerQuery)),
                    "reviewer" => dbQuery.Where(r => r.Reviewer != null && (r.Reviewer.FirstName + " " + r.Reviewer.LastName).ToLower().Contains(lowerQuery)),
                    "book" => dbQuery.Where(r => r.Book != null && r.Book.Title.ToLower().Contains(lowerQuery)),
                    _ => dbQuery
                };
            }

            return await dbQuery.OrderByDescending(r => r.CreatedAt).ToListAsync();
        }

        public async Task<BookReview?> GetByIdAsync(int id)
        {
            return await _context.BookReviews.FindAsync(id);
        }

        public async Task DeleteAsync(BookReview review)
        {
            _context.BookReviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }
}