using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
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
            return await _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Book)
                    .ThenInclude(b => b.Cover) // Правильний порядок!
                .Include(f => f.Book.Cover)
                .Include(f => f.Book.Owner)
                .Select(f => f.Book)
                .ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetFavoriteBooksAsync(int userId, FavoriteBooksQueryParameters parameters)
        {
            var query = _context.Favorites
                .Where(f => f.UserId == userId)
                .Include(f => f.Book)
                    .ThenInclude(b => b.Cover) // Правильний порядок!
                    .Include(f => f.Book.Owner)
                .Select(f => f.Book)
                .AsQueryable();

            if (!string.IsNullOrEmpty(parameters.Status))
            {
                query = query.Where(b => b.Status == parameters.Status);
            }

            var sortBy = parameters.SortBy?.Trim().ToLower();

            query = sortBy switch
            {
                "author" => parameters.IsDescending ? query.OrderByDescending(b => b.Author) : query.OrderBy(b => b.Author),
                "title" => parameters.IsDescending ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
                "year" => parameters.IsDescending ? query.OrderByDescending(b => b.Year) : query.OrderBy(b => b.Year),
                _ => query.OrderBy(b => b.Title)
            };

            return await query.ToListAsync();
        }

        public async Task ClearAllForUserAsync(int userId)
        {
            await _context.Favorites
                .Where(f => f.UserId == userId)
                .ExecuteDeleteAsync();
        }
    }
}