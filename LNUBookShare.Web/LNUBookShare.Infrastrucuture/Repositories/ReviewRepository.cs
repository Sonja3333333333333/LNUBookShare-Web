using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Infrastructure.Repositories;

public class ReviewRepository : IReviewRepository
{
    private readonly AppDbContext _context;

    public ReviewRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(BookReview review)
    {
        await _context.BookReviews.AddAsync(review);
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<BookReview>> GetByBookIdAsync(int bookId)
    {
        return await _context.BookReviews
            .Include(r => r.Reviewer) // Щоб вивести ім'я того, хто лишив відгук
            .Where(r => r.BookId == bookId)
            .OrderByDescending(r => r.CreatedAt) // Свіжі відгуки зверху
            .ToListAsync();
    }
}