// <copyright file="ICategoryRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface ICategoryRepository
    {
        Task<Category?> GetByIdAsync(int id);
        Task AddAsync(Category category);

        Task ClearAllAsync();
    }
}