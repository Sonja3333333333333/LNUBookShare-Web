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
    public class FavoriteController : Controller
    {
        private readonly IFavoriteService _favoriteService;
        private readonly UserManager<User> _userManager;
        private readonly ILogger<FavoriteController> _logger;

        public FavoriteController(IFavoriteService favoriteService, UserManager<User> userManager, ILogger<FavoriteController> logger)
        {
            _favoriteService = favoriteService;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] FavoriteBooksQueryParameters parameters)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // ВИПРАВЛЕНО: Залишили лише ОДИН виклик, який приймає параметри сортування/фільтрації (UC-2)
            var result = await _favoriteService.GetUserFavoriteBooksAsync(user.Id, parameters);

            // Передаємо параметри у ViewBag, щоб UI знав, які радіокнопки зараз вибрані
            ViewBag.QueryParameters = parameters;

            if (result.IsFailure)
            {
                _logger.LogWarning("Помилка отримання вподобань для User {UserId}: {Error}", user.Id, result.Error);
                TempData["ErrorMessage"] = result.Error;

                return View(new List<Book>());
            }

            return View(result.Value);
        }

        [HttpPost]
        public async Task<IActionResult> Toggle(int bookId, string returnUrl)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _favoriteService.ToggleFavoriteAsync(user.Id, bookId);

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
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _favoriteService.ClearUserFavoritesAsync(user.Id);

            if (result.IsFailure)
            {
                TempData["ErrorMessage"] = result.Error;
            }

            return RedirectToAction("Index");
        }
    }
}