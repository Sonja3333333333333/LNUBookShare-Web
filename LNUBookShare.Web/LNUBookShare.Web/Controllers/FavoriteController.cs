using System.Collections.Generic;
using System.Threading.Tasks;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Web.Controllers
{
    [Authorize]
    public class FavoriteController : BaseController
    {
        private readonly IFavoriteService _favoriteService;
        private readonly ILogger<FavoriteController> _logger;

        public FavoriteController(
            IFavoriteService favoriteService,
            UserManager<User> userManager,
            ILogger<FavoriteController> logger)
            : base(userManager)
        {
            _favoriteService = favoriteService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] FavoriteBooksQueryParameters parameters)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _favoriteService.GetUserFavoriteBooksAsync(userId.Value, parameters);

            ViewBag.QueryParameters = parameters;

            if (result.IsFailure)
            {
                _logger.LogWarning("Помилка отримання вподобань для User {UserId}: {Error}", userId.Value, result.Error);
                TempData["ErrorMessage"] = result.Error;

                return View(new List<Book>());
            }

            return View(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int bookId, string returnUrl)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _favoriteService.ToggleFavoriteAsync(userId.Value, bookId);

            if (result.IsFailure)
            {
                TempData["ErrorMessage"] = result.Error;
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Search", "Catalog");
        }

        [HttpPost]
        public async Task<IActionResult> ClearAll()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _favoriteService.ClearUserFavoritesAsync(userId.Value);

            if (result.IsFailure)
            {
                TempData["ErrorMessage"] = result.Error;
            }

            return RedirectToAction("Index");
        }
    }
}