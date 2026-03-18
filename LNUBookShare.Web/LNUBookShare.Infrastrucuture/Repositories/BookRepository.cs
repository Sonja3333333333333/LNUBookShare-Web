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
            return await _context.Books.Include(b => b.Owner).ToListAsync();
        }

        public async Task<Book?> GetByIdAsync(int id)
        {
            return await _context.Books.FindAsync(id);
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

        public async Task<IEnumerable<Book>> SearchBooksAsync(string keyword, string searchBy = "title")
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return new List<Book>();
            }

            var lowerKeyword = keyword.ToLower();
            var lower_searchBy = searchBy.ToLower();

            if (lower_searchBy == "author")
            {
                return await _context.Books
                    .Include(b => b.Owner)
                    .Where(b => EF.Functions.ILike(b.Author, $"%{lowerKeyword}%"))
                    .ToListAsync();
            }
            else if (lower_searchBy == "title")
            {
                return await _context.Books
                    .Include(b => b.Owner)
                    .Where(b => EF.Functions.ILike(b.Title, $"%{lowerKeyword}%"))
                    .ToListAsync();
            }
            else
            {
                return await _context.Books
                    .Include(b => b.Owner)
                    .Where(b => b.Isbn != null && b.Isbn.Contains(lowerKeyword))
                    .ToListAsync();
            }
        }
    }
}
