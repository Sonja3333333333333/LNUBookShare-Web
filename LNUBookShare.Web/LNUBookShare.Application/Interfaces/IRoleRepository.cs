// <copyright file="IRoleRepository.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IRoleRepository
    {
        Task AddAsync(Role role);

        Task ClearAllAsync();
    }
}
