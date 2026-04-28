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

        public async Task<IEnumerable<BookReview>> GetAllWithDetailsAsync()
        {
            return await _context.BookReviews
                .Include(r => r.Reviewer)
                .Include(r => r.Book)
                .AsNoTracking()
                .ToListAsync();
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