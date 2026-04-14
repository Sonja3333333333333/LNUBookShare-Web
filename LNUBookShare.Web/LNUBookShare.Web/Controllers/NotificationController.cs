using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LNUBookShare.Web.Controllers
{
    [Authorize]
    public class NotificationController : BaseController
    {
        private readonly INotificationService _notificationService;

        public NotificationController(
            UserManager<User> userManager,
            INotificationService notificationService)
            : base(userManager)
        {
            _notificationService = notificationService;
        }

        public async Task<IActionResult> Index(int userId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            if (userId != currentUserId)
            {
                return RedirectToAction(nameof(Index), new { userId = currentUserId });
            }

            var user_notifications = await _notificationService.GetUserNotificationsAsync(userId);

            if (!user_notifications.IsSuccess)
            {
                return NotFound(user_notifications.Error);
            }

            return View(user_notifications.Value);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsRead(int notificationId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _notificationService.MarkAsReadAsync(notificationId, (int)userId);

            if (result.IsSuccess)
            {
                TempData["StatusMessage"] = "Сповіщення видалено.";
            }
            else
            {
                TempData["ErrorMessage"] = string.Join(", ", result.Error);
            }

            return RedirectToAction(nameof(Index), new { userId = userId });
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
