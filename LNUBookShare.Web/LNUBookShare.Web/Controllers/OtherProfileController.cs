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
    public class OtherProfileController : BaseController
    {
        private readonly IOtherProfileService _otherProfileService;
        private readonly IFavoriteService _favoriteService;

        public OtherProfileController(
            UserManager<User> userManager,
            IOtherProfileService otherProfileService,
            IFavoriteService favoriteService)
            : base(userManager)
        {
            _otherProfileService = otherProfileService;
            _favoriteService = favoriteService;
        }

        public async Task<IActionResult> Index(int userId, string sortBy = "title", string statusFilter = "all")
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var user_details = await _otherProfileService.GetOtherUser(userId);
            if (!user_details.IsSuccess || user_details.Value is null)
            {
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
    }
}