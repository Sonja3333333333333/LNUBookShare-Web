using System.Security.Claims;
using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Models;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Controllers;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.Controllers
{
    public class ChatControllerTests
    {
        private readonly Mock<IChatService> _chatServiceMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly ChatController _controller;

        public ChatControllerTests()
        {
            _chatServiceMock = new Mock<IChatService>();

            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _controller = new ChatController(_chatServiceMock.Object, _userManagerMock.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "Bohdanovych"),
                new Claim("sub", "1")
            };

            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _userManagerMock.Setup(m => m.GetUserId(principal)).Returns("1");
            _userManagerMock.Setup(m => m.GetUserAsync(principal))
                .ReturnsAsync(new User { Id = 1, FirstName = "Max" });

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Fact]
        public async Task Index_ReturnsViewResult_WhenUserIsAuthorized()
        {
            // Arrange
            _chatServiceMock.Setup(s => s.GetConversationsAsync(1))
                .ReturnsAsync(Result<IEnumerable<ConversationDto>>.Success(new List<ConversationDto>()));

            // Act
            var result = await _controller.Index(null);

            // Assert
            // Якщо тут падає, значить result - це UnauthorizedResult (401)
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ChatViewModel>(viewResult.Model);
            Assert.Equal(1, model.CurrentUserId);
        }

        [Fact]
        public async Task SendMessage_ReturnsOk_ForAjaxRequest()
        {
            // Arrange
            _controller.Request.Headers["X-Requested-With"] = "XMLHttpRequest";
            _chatServiceMock.Setup(s => s.SendMessageAsync(1, 2, "Test"))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _controller.SendMessage(2, "Test");

            // Assert
            Assert.IsType<OkResult>(result);
        }

        [Fact]
        public async Task SendMessage_Redirects_ForNormalRequest()
        {
            // Arrange 
            _chatServiceMock.Setup(s => s.SendMessageAsync(1, 2, "Test"))
                .ReturnsAsync(Result.Success());

            // Act
            var result = await _controller.SendMessage(2, "Test");

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
        }

        [Fact]
        public async Task Index_LoadsSelectedUser_WhenReceiverIdProvided()
        {
            // Arrange
            int rId = 2;
            _chatServiceMock.Setup(s => s.GetConversationsAsync(1))
                .ReturnsAsync(Result<IEnumerable<ConversationDto>>.Success(new List<ConversationDto>()));
            _chatServiceMock.Setup(s => s.GetChatHistoryAsync(1, rId))
                .ReturnsAsync(Result<IEnumerable<ChatMessage>>.Success(new List<ChatMessage>()));

            _userManagerMock.Setup(m => m.FindByIdAsync("2"))
                .ReturnsAsync(new User { Id = 2, FirstName = "Sofa" });

            // Act
            var result = await _controller.Index(rId);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ChatViewModel>(viewResult.Model);
            Assert.NotNull(model.SelectedUser);
            if (model.SelectedUser != null)
            {
                Assert.Equal("Sofa", model.SelectedUser.FirstName);
            }
        }
    }
}