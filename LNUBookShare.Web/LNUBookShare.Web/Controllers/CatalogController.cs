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
        private readonly IBookDetailsService _bookDetailsService;
        private readonly IReviewService _reviewService; // Додаємо наш сервіс відгуків

        public CatalogController(
            IBookSearchService searchService,
            ILogger<CatalogController> logger,
            UserManager<User> userManager,
            IFavoriteService favoriteService,
            IBookDetailsService bookDetailsService,
            IReviewService reviewService) // Ін'єктуємо сюди
        {
            _searchService = searchService;
            _logger = logger;
            _userManager = userManager;
            _favoriteService = favoriteService;
            _bookDetailsService = bookDetailsService;
            _reviewService = reviewService;
        }

        public async Task<IActionResult> Search(string query, string searchBy = "title", string sortBy = "title", string statusFilter = "all")
        {
            IEnumerable<Book> results;
            var currentUser = await _userManager.GetUserAsync(User);

            if (!string.IsNullOrWhiteSpace(query))
            {
                results = await _searchService.SearchAsync(query, searchBy, sortBy, statusFilter);
                _logger.LogInformation("Пошук: {Query}", query);
            }
            else
            {
                if (currentUser != null)
                {
                    results = await _searchService.GetRecommendationsAsync(currentUser.FacultyId, currentUser.Id, sortBy, statusFilter);
                    ViewBag.IsRecommendation = true;
                }
                else
                {
                    results = await _searchService.SearchAsync(string.Empty, "title", sortBy, statusFilter);
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

        public async Task<IActionResult> Details(int id, string? returnUrl = null)
        {
            var bookResult = await _bookDetailsService.GetBookDetailsAsync(id);

            if (bookResult.IsFailure)
            {
                return NotFound(bookResult.Error);
            }

            var results = bookResult.Value;

            // Отримуємо актуальний середній рейтинг через наш новий сервіс
            var avgRating = await _reviewService.CalculateAverageRatingAsync(id);
            var reviews = await _reviewService.GetBookReviewsAsync(id);

            ViewBag.ReturnUrl = returnUrl;

            var model = new BookDetailsViewModel
            {
                BookId = results.BookId,
                Title = results.Title,
                Author = results.Author,
                Owner = results.Owner,
                Status = results.Status,
                BookReviews = reviews, // Використовуємо список із сервісу
                AverageRating = avgRating, // Використовуємо рейтинг із сервісу
                Category = results.Category,
                Language = results.Language,
                Publisher = results.Publisher,
                Isbn = results.Isbn,
                Cover = results.Cover,
                FavoritedBookIds = await GetUserFavoriteIdsAsync(),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int bookId, int rating, string comment)
        {
            var userIdString = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized();
            }

            int userId = int.Parse(userIdString);

            // Викликаємо логіку сервісу (валідація 1-5 та логування там уже є)
            var result = await _reviewService.AddReviewAsync(bookId, userId, rating, comment);

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Error;
            }
            else
            {
                TempData["SuccessMessage"] = "Ваш відгук успішно додано!";
            }

            return RedirectToAction(nameof(Details), new { id = bookId });
        }

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