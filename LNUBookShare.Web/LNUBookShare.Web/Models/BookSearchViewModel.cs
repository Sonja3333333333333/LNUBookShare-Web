// <copyright file="BookSearchViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using LNUBookShare.Domain.Entities;
namespace LNUBookShare.Web.Models
{
    public class BookSearchViewModel
    {
        public string? SearchQuery { get; set; }
        public IEnumerable<Book> Books { get; set; } = new List<Book>();

        public string SortBy { get; set; } = "title";
        public string StatusFilter { get; set; } = "all";

        public int TotalCount
        {
            get
            {
                return Books.Count();
            }
        }

        public string SearchBy { get; set; } = "title";

        public HashSet<int> FavoritedBookIds { get; set; } = new HashSet<int>();
    }
}
