// <copyright file="CatalogController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
using System.Globalization;
using System.Security.Claims;
using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
namespace LNUBookShare.Web.Controllers
{
    [Authorize]
    public class CatalogController : Controller
    {
        private readonly IBookSearchService _searchService;
        private readonly ILogger<CatalogController> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IFavoriteService _favoriteService;
        private readonly IBookDetailsService _bookDetailsService;
        public CatalogController(IBookSearchService searchService, ILogger<CatalogController> logger, UserManager<User> userManager, IFavoriteService favoriteService, IBookDetailsService bookDetailsService)
        {
            _searchService = searchService;
            _logger = logger;
            _userManager = userManager;
            _favoriteService = favoriteService;
            _bookDetailsService = bookDetailsService;
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

            var model = new BookSearchViewModel
            {
                SearchQuery = query,
                Books = results,
                SearchBy = searchBy,
                SortBy = sortBy,
                StatusFilter = statusFilter,
                FavoritedBookIds = await GetUserFavoriteIdsAsync(),
            };

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var book = await _bookDetailsService.GetBookDetailsAsync(id);

            if (book.IsFailure)
            {
                return NotFound(book.Error);
            }

            var results = book.Value;

            var model = new BookDetailsViewModel
            {
                BookId = results.BookId,
                Title = results.Title,
                Author = results.Author,
                Owner = results.Owner,
                Status = results.Status,
                BookReviews = results.BookReviews,
                Category = results.Category,
                Language = results.Language,
                Publisher = results.Publisher,
                Isbn = results.Isbn,
                Cover = results.Cover,
                FavoritedBookIds = await GetUserFavoriteIdsAsync(),
            };
            return View(model);
        }

        // логіку отримання улюблених книг користувача
        // можна винести в окремий метод, щоб не дублювати код у різних діях контролера
        // також _FavoriteButton який викликаться на різних сторінках(Search та Details), може використовувати цей метод для відображення кнопки вподобати
        private async Task<HashSet<int>> GetUserFavoriteIdsAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
            {
                return new HashSet<int>();
            }

            var favResult = await _favoriteService.GetUserFavoriteBookIdsAsync(currentUser.Id);
            return favResult.IsSuccess ? favResult.Value.ToHashSet() : new HashSet<int>();
        }
    }
}
