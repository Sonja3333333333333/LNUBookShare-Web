using LNUBookShare.Application.Common;
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
    public class OtherProfileController : BaseController
    {
        private readonly IOtherProfileService _otherProfileService;
        private readonly IFavoriteService _favoriteService;
        private readonly IReportService _reportService;
        private readonly ILogger<OtherProfileController> _logger;

        public OtherProfileController(
            UserManager<User> userManager,
            IOtherProfileService otherProfileService,
            IFavoriteService favoriteService,
            IReportService reportService,
            ILogger<OtherProfileController> logger)
            : base(userManager)
        {
            _otherProfileService = otherProfileService;
            _favoriteService = favoriteService;
            _reportService = reportService;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int userId, string sortBy = "title", string statusFilter = "all")
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                _logger.LogWarning("\x1b[31m[AUTH] Спроба доступу до профілю {UserId} неавторизованим користувачем.\x1b[0m", userId);
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("\x1b[36m[PROFILE] Користувач {CurrentUserId} переглядає профіль {TargetUserId}.\x1b[0m", currentUserId, userId);

            var user_details = await _otherProfileService.GetOtherUser(userId);
            if (!user_details.IsSuccess || user_details.Value is null)
            {
                _logger.LogWarning("\x1b[33m[PROFILE] Користувача з ID {UserId} не знайдено.\x1b[0m", userId);
                return NotFound(user_details.Error);
            }

            var v_user = user_details.Value;
            var books = await _otherProfileService.GetOtherUserBooks(userId, sortBy, statusFilter);
            var vbooks = books.Value;

            HashSet<int> favBooksIds = new HashSet<int>();

            var favResult = await _favoriteService.GetUserFavoriteBookIdsAsync(currentUserId.Value);
            if (favResult.IsSuccess)
            {
                favBooksIds = favResult.Value.ToHashSet();
            }

            var model = new OtherProfileViewModel
            {
                UserId = v_user.Id,
                FirstName = v_user.FirstName ?? "Невідомо",
                LastName = v_user.LastName ?? " ",
                Email = v_user.Email ?? "Невідомо",
                Faculty = v_user.Faculty?.FacultyName ?? "Не вказано",
                Avatar = v_user.Avatar,
                Books = vbooks,
                FavoritedBookIds = favBooksIds,
                SortBy = sortBy,
                StatusFilter = statusFilter,
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Report(int reportedUserId, string reason)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            _logger.LogInformation("\x1b[35m[REPORT] Спроба репорту від {SenderId} на {TargetId}. Причина: {Reason}\x1b[0m", currentUserId, reportedUserId, reason);

            var result = await _reportService.CreateReportAsync(currentUserId.Value, reportedUserId, reason);

            if (result.IsSuccess)
            {
                _logger.LogInformation("\x1b[32m[REPORT SUCCESS] Скарга на {TargetId} успішно збережена в базу.\x1b[0m", reportedUserId);
                TempData["SuccessMessage"] = "Вашу скаргу надіслано адміністрації.";
            }
            else
            {
                _logger.LogWarning("\x1b[31m[REPORT FAILED] Не вдалося створити скаргу: {Error}\x1b[0m", result.Error);
                TempData["ErrorMessage"] = result.Error;
            }

            return RedirectToAction("Index", new { userId = reportedUserId });
        }
    }
}