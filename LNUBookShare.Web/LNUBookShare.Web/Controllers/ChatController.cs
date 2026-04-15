using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LNUBookShare.Web.Controllers
{
    [Authorize]
    public class ChatController : BaseController
    {
        private readonly IChatService _chatService;
        private readonly UserManager<User> _userManager;

        public ChatController(IChatService chatService, UserManager<User> userManager)
            : base(userManager)
        {
            _chatService = chatService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Index(int? receiverId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var convResult = await _chatService.GetConversationsAsync(userId.Value);
            var model = new ChatViewModel
            {
                CurrentUserId = userId.Value,
                Conversations = convResult.IsSuccess ? convResult.Value : new List<LNUBookShare.Application.Models.ConversationDto>(),
            };

            if (receiverId.HasValue)
            {
                var historyResult = await _chatService.GetChatHistoryAsync(userId.Value, receiverId.Value);
                if (historyResult.IsSuccess)
                {
                    model.Messages = historyResult.Value;
                }

                model.SelectedUser = await _userManager.FindByIdAsync(receiverId.Value.ToString());
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int receiverId, string content)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _chatService.SendMessageAsync(userId.Value, receiverId, content);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return result.IsSuccess ? Ok() : BadRequest(result.Error);
            }

            return RedirectToAction(nameof(Index), new { receiverId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConversation(int receiverId)
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _chatService.DeleteConversationAsync(userId.Value, receiverId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                if (result.IsSuccess)
                {
                    return Ok();
                }

                return BadRequest(result.Error);
            }

            if (!result.IsSuccess)
            {
                TempData["ErrorMessage"] = result.Error;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}