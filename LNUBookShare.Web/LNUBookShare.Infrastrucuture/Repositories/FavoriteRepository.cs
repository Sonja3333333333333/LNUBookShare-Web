using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Infrastructure.Repositories
{
    public class FavoriteRepository : IFavoriteRepository
    {
        private readonly AppDbContext _context;

        public FavoriteRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> ExistsAsync(int userId, int bookId)
        {
            return await _context.Favorites.AnyAsync(f => f.UserId == userId && f.BookId == bookId);
        }

        public async Task AddAsync(Favorite favorite)
        {
            await _context.Favorites.AddAsync(favorite);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveAsync(int userId, int bookId)
        {
            var favorite = await _context.Favorites
                .FirstOrDefaultAsync(f => f.UserId == userId && f.BookId == bookId);

            if (favorite != null)
            {
                _context.Favorites.Remove(favorite);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<int>> GetUserFavoriteBookIdsAsync(int userId)
        {
            return await _context.Favorites
                .Where(f => f.UserId == userId)
                .Select(f => f.BookId)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetUserFavoriteBooksAsync(int userId)
        {
            // Шукаємо записи вподобань для юзера і за допомогою Include підтягуємо саму книгу та її обкладинку
            return await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Book)
                    .ThenInclude(b => b.Cover) // Щоб вивести картинку
                .Select(f => f.Book) // Повертаємо тільки об'єкти Book
                .ToListAsync();
        }
    }
}
