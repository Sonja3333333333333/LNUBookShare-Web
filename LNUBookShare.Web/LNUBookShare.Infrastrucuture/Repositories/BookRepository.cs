// <copyright file="BookRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Models;
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
            return await _context.Books.Include(b => b.Category).ToListAsync();
        }

        public async Task<IEnumerable<Book>> GetFilteredAsync(BookFilterParams filterParams)
        {
            IQueryable<Book> query = _context.Books.Include(b => b.Category);

            if (!string.IsNullOrEmpty(filterParams.Status))
            {
                query = query.Where(b => b.Status == filterParams.Status);
            }

            if (filterParams.CategoryId.HasValue)
            {
                query = query.Where(b => b.CategoryId == filterParams.CategoryId.Value);
            }

            query = filterParams.SortBy?.ToLower() switch
            {
                "year" => query.OrderByDescending(b => b.Year),
                "title" => query.OrderBy(b => b.Title),
                _ => query.OrderBy(b => b.Title), 
            };

            return await query.ToListAsync();
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
    }
}