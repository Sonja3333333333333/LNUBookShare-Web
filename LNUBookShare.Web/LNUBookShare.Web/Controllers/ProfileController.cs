using System.Threading.Tasks;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Web.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly IProfileService _profileService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            UserManager<User> userManager,
            IProfileService profileService,
            ILogger<ProfileController> logger)
        {
            _userManager = userManager;
            _profileService = profileService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            _logger.LogInformation("Користувач зайшов у свій кабінет.");

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Логіка завантаження даних для відображення профілю з гілки main
            var result = await _profileService.GetUserProfileAsync(user.Id);

            var model = new UserProfileViewModel
            {
                UserId = user.Id,
                FirstName = user.FirstName ?? "Не вказано",
                LastName = user.LastName ?? "Не вказано",
                Email = user.Email ?? "Не вказано",
                FacultyName = "Тут буде назва факультету",
                AvatarPath = user.Avatar?.ImagePath
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> AddBook(AddBookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Перевірте правильність заповнення форми.";
                return RedirectToAction("Index");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var book = new Book
            {
                Title = model.Title,
                Author = model.Author,
                Year = model.Year,
                Publisher = model.Publisher,
                Language = model.Language,
                CategoryId = model.CategoryId,
                OwnerId = user.Id
            };

            var result = await _profileService.AddBookToProfileAsync(book);

            if (result.IsSuccess)
            {
                TempData["SuccessMessage"] = "Книгу успішно додано до вашого профілю!";
            }
            else
            {
                TempData["ErrorMessage"] = result.Error;
            }

            return RedirectToAction("Index");
        }
    }
}