// <copyright file="CatalogController.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LNUBookShare.Web.Controllers
{
    [Authorize]
    public class CatalogController : BaseController
    {
        private readonly IBookSearchService _searchService;
        private readonly ILogger<CatalogController> _logger;
        private readonly IFavoriteService _favoriteService;
        private readonly IBookDetailsService _bookDetailsService;
        private readonly IReviewService _reviewService;
        private readonly IReservationService _reservationService;

        public CatalogController(
            IBookSearchService searchService,
            ILogger<CatalogController> logger,
            UserManager<User> userManager,
            IFavoriteService favoriteService,
            IBookDetailsService bookDetailsService,
            IReviewService reviewService,
            IReservationService reservationService)
            : base(userManager)
        {
            _searchService = searchService;
            _logger = logger;
            _favoriteService = favoriteService;
            _bookDetailsService = bookDetailsService;
            _reviewService = reviewService;
            _reservationService = reservationService;
        }

        [HttpGet]
        public async Task<IActionResult> Search(string query, string searchBy = "title", string sortBy = "title", string statusFilter = "all")
        {
            IEnumerable<Book> results;
            var currentUser = await GetCurrentUserAsync();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var searchResult = await _searchService.SearchAsync(query, searchBy, sortBy, statusFilter);
                results = searchResult.IsSuccess ? searchResult.Value : Enumerable.Empty<Book>();
            }
            else
            {
                if (currentUser != null)
                {
                    var recResult = await _searchService.GetRecommendationsAsync(currentUser.FacultyId, currentUser.Id, sortBy, statusFilter);
                    results = recResult.IsSuccess ? recResult.Value : Enumerable.Empty<Book>();
                    ViewBag.IsRecommendation = true;
                }
                else
                {
                    var searchResult = await _searchService.SearchAsync(string.Empty, "title", sortBy, statusFilter);
                    results = searchResult.IsSuccess ? searchResult.Value : Enumerable.Empty<Book>();
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

        [HttpGet]
        public async Task<IActionResult> Details(int id, string? returnUrl = null)
        {
            var bookResult = await _bookDetailsService.GetBookDetailsAsync(id);
            if (bookResult.IsFailure)
            {
                return NotFound(bookResult.Error);
            }

            var book = bookResult.Value;

            var avgRatingResult = await _reviewService.CalculateAverageRatingAsync(id);
            var avgRating = avgRatingResult.IsSuccess ? avgRatingResult.Value : 0.0;

            var reviewsResult = await _reviewService.GetBookReviewsAsync(id);
            var reviews = reviewsResult.IsSuccess ? reviewsResult.Value : Enumerable.Empty<BookReview>();

            bool isInQueue = false;
            int queuePosition = 0;
            bool hasReviewed = false;
            var currentUser = await GetCurrentUserAsync();

            if (currentUser != null)
            {
                var queueStatusResult = await _reservationService.IsUserInQueueAsync(id, currentUser.Id);
                isInQueue = queueStatusResult.IsSuccess && queueStatusResult.Value;
                if (isInQueue)
                {
                    var positionResult = await _reservationService.GetQueuePositionAsync(id, currentUser.Id);
                    queuePosition = positionResult.IsSuccess ? positionResult.Value : 0;
                }

                var hasReviewedResult = await _reviewService.HasUserReviewedAsync(id, currentUser.Id);
                hasReviewed = hasReviewedResult.IsSuccess && hasReviewedResult.Value;
            }

            ViewBag.ReturnUrl = returnUrl;

            var model = new BookDetailsViewModel
            {
                BookId = book.BookId,
                Title = book.Title,
                Author = book.Author,
                Owner = book.Owner,
                Status = book.Status,
                Category = book.Category,
                Language = book.Language,
                Publisher = book.Publisher,
                Isbn = book.Isbn,
                Cover = book.Cover,
                BookReviews = reviews,
                AverageRating = avgRating,
                IsInQueue = isInQueue,
                QueuePosition = queuePosition,
                HasUserReviewed = hasReviewed, // Передаємо у модель
                FavoritedBookIds = await GetUserFavoriteIdsAsync(),
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reserve(int bookId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _reservationService.ReserveBookAsync(bookId, currentUserId.Value);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Ви успішно забронювали книгу!";
            }
            else
            {
                TempData["ErrorMessage"] = result.Error;
            }

            return RedirectToAction(nameof(Details), new { id = bookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> JoinQueue(int bookId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _reservationService.JoinQueueAsync(bookId, currentUserId.Value);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Ви успішно стали в чергу!";
            }
            else
            {
                TempData["ErrorMessage"] = result.Error;
            }

            return RedirectToAction(nameof(Details), new { id = bookId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int bookId, int rating, string comment)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return Unauthorized();
            }

            var result = await _reviewService.AddReviewAsync(bookId, currentUserId.Value, rating, comment);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Ваш відгук додано!";
            }
            else
            {
                TempData["ErrorMessage"] = result.Error;
            }

            return RedirectToAction(nameof(Details), new { id = bookId });
        }

        private async Task<HashSet<int>> GetUserFavoriteIdsAsync()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return new HashSet<int>();
            }

            var favResult = await _favoriteService.GetUserFavoriteBookIdsAsync(currentUserId.Value);
            return favResult.IsSuccess ? favResult.Value.ToHashSet() : new HashSet<int>();
        }
    }
}