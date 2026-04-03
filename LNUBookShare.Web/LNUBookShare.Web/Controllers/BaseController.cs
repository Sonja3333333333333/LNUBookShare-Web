using LNUBookShare.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LNUBookShare.Web.Controllers
{
    public abstract class BaseController : Controller
    {
        private readonly UserManager<User> _userManager;

        protected BaseController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        protected async Task<User?> GetCurrentUserAsync()
        {
            return await _userManager.GetUserAsync(User);
        }

        protected int? GetCurrentUserId()
        {
            var userIdString = _userManager.GetUserId(User);
            if (int.TryParse(userIdString, out var userId))
            {
                return userId;
            }

            return null;
        }
    }
}