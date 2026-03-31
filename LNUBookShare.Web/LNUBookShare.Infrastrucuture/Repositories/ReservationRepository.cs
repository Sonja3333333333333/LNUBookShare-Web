using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Infrastructure.Repositories
{
    public class ReservationRepository : IReservationRepository
    {
        private readonly AppDbContext _context;

        public ReservationRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ReservationQueue entry)
        {
            await _context.ReservationQueues.AddAsync(entry);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(int bookId, int userId)
        {
            return await _context.ReservationQueues
                .AnyAsync(q => q.BookId == bookId && q.UserId == userId);
        }

        public async Task<int> GetPositionAsync(int bookId, int userId)
        {
            var queue = await _context.ReservationQueues
                .Where(q => q.BookId == bookId)
                .OrderBy(q => q.CreatedAt)
                .Select(q => q.UserId)
                .ToListAsync();

            return queue.IndexOf(userId) + 1;
        }
    }
}