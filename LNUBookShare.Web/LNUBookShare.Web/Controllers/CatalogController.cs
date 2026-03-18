// <copyright file="CatalogController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Models;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LNUBookShare.Web.Controllers
{
    public class CatalogController : Controller
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(
            IBookRepository bookRepository,
            ICategoryRepository categoryRepository,
            ILogger<CatalogController> logger)
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index([FromQuery] BookFilterParams filterParams)
        {
            var validSortValues = new[] { "title", "year" };
            if (!string.IsNullOrEmpty(filterParams.SortBy) &&
                !validSortValues.Contains(filterParams.SortBy.ToLower()))
            {
                _logger.LogWarning("Невалідний параметр сортування: {SortBy}. Використовується default.", filterParams.SortBy);
                filterParams.SortBy = "title";
            }

            _logger.LogInformation(
                "Застосовано фільтри: Статус={Status}, Сортування={SortBy}",
                filterParams.Status ?? "всі",
                filterParams.SortBy);

            var books = await _bookRepository.GetFilteredAsync(filterParams);
            var categories = await _categoryRepository.GetAllAsync();

            var categoryItems = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName,
            }).Prepend(new SelectListItem { Value = "", Text = "Всі категорії" });

            var viewModel = new CatalogViewModel
            {
                Books = books,
                FilterParams = filterParams,
                Categories = categoryItems,
            };

            return View(viewModel);
        }
    }
}