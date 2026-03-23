// <copyright file="BookRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

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
            var book = await _context.Books
                .Include(b => b.Owner)
                .Include(b => b.Cover)
                .Include(b => b.Category)
                .Include(b => b.BookReviews)
                    .ThenInclude(br => br.Reviewer)
                .FirstOrDefaultAsync(b => b.BookId == id);

            return book;
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

        public async Task<IEnumerable<Book>> SearchBooksAsync(string keyword, string searchBy = "title", string sortBy = "title", string statusFilter = "all")
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<Book>();
            }

            var lowerKeyword = keyword.ToLower();
            var lower_searchBy = searchBy.ToLower();

            if (lower_searchBy == "author")
            {
                var query = _context.Books
                    .Include(b => b.Owner)
                    .Include(b => b.Cover)
                    .Where(b => EF.Functions.ILike(b.Author, $"%{lowerKeyword}%"));

                return await ApplySortingAndFiltering(query, sortBy, statusFilter).ToListAsync();
            }
            else if (lower_searchBy == "title")
            {
                var query = _context.Books
                    .Include(b => b.Owner)
                    .Include(b => b.Cover)
                    .Where(b => EF.Functions.ILike(b.Title, $"%{lowerKeyword}%"));

                return await ApplySortingAndFiltering(query, sortBy, statusFilter).ToListAsync();
            }
            else
            {
                var query = _context.Books
                    .Include(b => b.Owner)
                    .Include(b => b.Cover)
                    .Where(b => b.Isbn != null && b.Isbn.Contains(lowerKeyword));

                return await ApplySortingAndFiltering(query, sortBy, statusFilter).ToListAsync();
            }
        }

        public async Task<IEnumerable<Book>> GetRecommendationsAsync(int facultyId, int currentUserId, string sortBy = "title", string statusFilter = "all")
        {
            var query = _context.Books
                .Include(b => b.Owner)
                .Include(b => b.Cover)
                .Where(b => b.Owner.FacultyId == facultyId && b.OwnerId != currentUserId);

            return await ApplySortingAndFiltering(query, sortBy, statusFilter).ToListAsync();
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
