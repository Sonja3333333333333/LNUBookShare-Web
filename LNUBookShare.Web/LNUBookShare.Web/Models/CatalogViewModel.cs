// <copyright file="CatalogViewModel.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using LNUBookShare.Application.Models;
using LNUBookShare.Domain.Entities;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LNUBookShare.Web.Models
{
    public class CatalogViewModel
    {
        public IEnumerable<Book> Books { get; set; } = new List<Book>();
        public BookFilterParams FilterParams { get; set; } = new BookFilterParams();
        public IEnumerable<SelectListItem> Categories { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Statuses { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "", Text = "Всі" },
            new SelectListItem { Value = "Available", Text = "Доступна" },
            new SelectListItem { Value = "Borrowed", Text = "Позичена" },
        };
        public IEnumerable<SelectListItem> SortOptions { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "title", Text = "За назвою" },
            new SelectListItem { Value = "year", Text = "За роком (новіші)" },
        };
    }
}