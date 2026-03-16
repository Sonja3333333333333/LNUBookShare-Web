// <copyright file="IUserRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllAsync(); // Read (всі)
        Task<User?> GetByIdAsync(int id); // Read (одна)
        Task AddAsync(User user); // Create
        Task UpdateAsync(User user); // Update
        Task DeleteAsync(User user); // Delete

        Task ClearAllAsync();
    }
}