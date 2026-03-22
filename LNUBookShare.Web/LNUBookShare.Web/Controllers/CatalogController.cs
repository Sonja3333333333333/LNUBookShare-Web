// <copyright file="CatalogController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

using System.Security.Claims;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace LNUBookShare.Web.Controllers
{
    [Authorize]
    public class CatalogController : Controller
    {
        private readonly IBookSearchService _searchService;
        private readonly ILogger<CatalogController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IFavoriteService _favoriteService;

        public CatalogController(IBookSearchService searchService, ILogger<CatalogController> logger, UserManager<User> userManager, IFavoriteService favoriteService)
        {
            _searchService = searchService;
            _logger = logger;
            _userManager = userManager;
            _favoriteService = favoriteService;
        }

        public async Task<IActionResult> Search(string query, string searchBy = "title", string sortBy = "title", string statusFilter = "all")
        {
            IEnumerable<Book> results;
            var currentUser = await _userManager.GetUserAsync(User);

            if (!string.IsNullOrWhiteSpace(query))
            {
                results = await _searchService.SearchAsync(query, searchBy, sortBy, statusFilter);
                _logger.LogInformation(
                    "Користувач здійснив пошук. Запит: '{Query}', Критерій: {SearchBy}, Сортування: {SortBy}, Фільтр статусу: {StatusFilter}", query, searchBy, sortBy, statusFilter);
                    }
            else
            {
                if (currentUser != null)
                {
                    results = await _searchService.GetRecommendationsAsync(currentUser.FacultyId, currentUser.Id, sortBy, statusFilter);
                    _logger.LogInformation(
                "Згенеровано рекомендації для користувача ID: {UserId} (Факультет ID: {FacultyId}). Сортування: {SortBy}, Фільтр статусу: {StatusFilter}", currentUser.Id, currentUser.FacultyId, sortBy, statusFilter);
                    ViewBag.IsRecommendation = true;
                }
                else
                {
                    results = await _searchService.SearchAsync(string.Empty, "title", sortBy, statusFilter);
                    _logger.LogInformation(
                "Перегляд усіх книг каталогу. Сортування: {SortBy}, Фільтр статусу: {StatusFilter}", sortBy, statusFilter);
                }
            }

            var favoritedIds = new HashSet<int>();
            if (currentUser != null)
            {
                var favResult = await _favoriteService.GetUserFavoriteBookIdsAsync(currentUser.Id);
                if (favResult.IsSuccess)
                {
                    favoritedIds = favResult.Value.ToHashSet(); // Якщо успіх, беремо Value
                }
            }

            var model = new BookSearchViewModel
            {
                SearchQuery = query,
                Books = results,
                SearchBy = searchBy,
                SortBy = sortBy,
                StatusFilter = statusFilter,
                FavoritedBookIds = favoritedIds,
            };

            return View(model);
        }
    }
}
