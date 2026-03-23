// <copyright file="IBookRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync(); // Read (всі)
        Task<Book?> GetByIdAsync(int id); // Read (одна)
        Task<Book?> GetByIdMoreDetailsAsync(int id); // Read (одна з деталями)
        Task AddAsync(Book book); // Create
        Task UpdateAsync(Book book); // Update
        Task DeleteAsync(Book book); // Delete

        Task ClearAllAsync();

        Task<IEnumerable<Book>> SearchBooksAsync(string searchBy, string query, string sortBy = "title", string statusFilter = "all"); // search
        Task<IEnumerable<Book>> GetRecommendationsAsync(int facultyId, int currentUserId, string sortBy = "title", string statusFilter = "all"); // recs
    }
}