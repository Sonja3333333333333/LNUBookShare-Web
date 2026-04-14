using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LNUBookShare.Web.Controllers
{
    [Authorize]
    public class NotificationsController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(
            UserManager<User> userManager,
            INotificationService notificationService)
            : base(userManager)
        {
            _notificationService = notificationService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _notificationService.GetUserNotificationsAsync(userId.Value);
            var notifications = result.IsSuccess
                ? result.Value
                : Enumerable.Empty<Notification>();

            return View(notifications);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var result = await _notificationService.MarkAsReadAsync(id, userId.Value);

            if (result.IsFailure)
            {
                TempData["ErrorMessage"] = result.Error;
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAll()
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var notificationsResult = await _notificationService
                .GetUserNotificationsAsync(userId.Value);

            if (notificationsResult.IsSuccess)
            {
                foreach (var notification in notificationsResult.Value)
                {
                    await _notificationService.MarkAsReadAsync(
                        notification.Id, userId.Value);
                }
            }

            TempData["SuccessMessage"] = "Усі сповіщення видалено.";
            return RedirectToAction(nameof(Index));
        }
    }
}