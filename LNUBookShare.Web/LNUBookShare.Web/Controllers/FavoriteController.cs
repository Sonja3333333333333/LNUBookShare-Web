using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Викликаємо сервіс
            var result = await _favoriteService.GetUserFavoriteBooksAsync(user.Id);

            if (result.IsFailure)
            {
                // Якщо сталася логічна помилка, виводимо порожній список і логуємо
                _logger.LogWarning("Помилка отримання вподобань для User {UserId}: {Error}", user.Id, result.Error);
                return View(new List<Book>());
            }

            // Передаємо успішний список книг у View
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

            // Викликаємо сервіс (наша бізнес-логіка)
            var result = await _favoriteService.ToggleFavoriteAsync(user.Id, bookId);

            if (result.IsFailure)
            {
                // Якщо помилка, можемо записати її в TempData, щоб показати на екрані
                TempData["ErrorMessage"] = result.Error;
            }

            // Повертаємо користувача на ту сторінку, де він був (зі збереженням пошуку і фільтрів)
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Search", "Catalog");
        }
    }
}