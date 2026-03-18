// <copyright file="BookFilterParams.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace LNUBookShare.Application.Models
{
    public class BookFilterParams
    {
        public string? SortBy { get; set; } = "title"; 
        public int? CategoryId { get; set; }
        public string? Status { get; set; } 
    }
}