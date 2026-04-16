using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using System.Security.Claims;
using Xunit;

namespace LNUBookShare.UnitTests.Notification_tests
{
    public class NotificationControllerTests
    {
        private readonly Mock<INotificationService> _notificationServiceMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly NotificationController _controller;

        public NotificationControllerTests()
        {
            _notificationServiceMock = new Mock<INotificationService>();

            var store = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(
                store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _controller = new NotificationController(
                _userManagerMock.Object,
                _notificationServiceMock.Object);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var principal = new ClaimsPrincipal(identity);

            _userManagerMock.Setup(m => m.GetUserId(principal)).Returns("1");

            var httpContext = new DefaultHttpContext { User = principal };
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext,
            };
            _controller.TempData = new TempDataDictionary(
                httpContext,
                Mock.Of<ITempDataProvider>());
        }

        [Fact]
        public async Task DeleteAll_WhenUserHasNotifications_DeletesAllAndRedirects()
        {
            
            var notifications = new List<Notification>
            {
                new Notification { Id = 1, UserId = 1, Message = "Сповіщення 1" },
                new Notification { Id = 2, UserId = 1, Message = "Сповіщення 2" },
            };

            _notificationServiceMock
                .Setup(s => s.GetUserNotificationsAsync(1))
                .ReturnsAsync(Result<IEnumerable<Notification>>.Success(notifications));

            _notificationServiceMock
                .Setup(s => s.MarkAsReadAsync(It.IsAny<int>(), 1))
                .ReturnsAsync(Result.Success());

            
            var result = await _controller.DeleteAll();

            
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            _notificationServiceMock.Verify(
                s => s.MarkAsReadAsync(It.IsAny<int>(), 1),
                Times.Exactly(2));

            Assert.Equal("Усі сповіщення видалено.", _controller.TempData["SuccessMessage"]);
        }

        [Fact]
        public async Task DeleteAll_WhenNoNotifications_StillRedirects()
        {
            
            _notificationServiceMock
                .Setup(s => s.GetUserNotificationsAsync(1))
                .ReturnsAsync(Result<IEnumerable<Notification>>.Success(
                    new List<Notification>()));

           
            var result = await _controller.DeleteAll();

            
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            _notificationServiceMock.Verify(
                s => s.MarkAsReadAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }

        [Fact]
        public async Task DeleteAll_WhenServiceFails_StillRedirects()
        {
            
            _notificationServiceMock
                .Setup(s => s.GetUserNotificationsAsync(1))
                .ReturnsAsync(Result<IEnumerable<Notification>>.Failure("Помилка"));

            // Act
            var result = await _controller.DeleteAll();

            // Assert
            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);

            _notificationServiceMock.Verify(
                s => s.MarkAsReadAsync(It.IsAny<int>(), It.IsAny<int>()),
                Times.Never);
        }
    }
}