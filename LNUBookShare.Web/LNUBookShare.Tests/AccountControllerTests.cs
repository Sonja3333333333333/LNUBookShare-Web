using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Controllers;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace LNUBookShare.Tests.Controllers
{
    public class AccountControllerTests
    {
        private readonly Mock<UserManager<User>> _mockUserManager;
        private readonly Mock<SignInManager<User>> _mockSignInManager;
        private readonly Mock<ILogger<AccountController>> _mockLogger;
        private readonly AccountController _controller;

        public AccountControllerTests()
        {
            var store = new Mock<IUserStore<User>>();
            _mockUserManager = new Mock<UserManager<User>>(store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            var contextAccessor = new Mock<IHttpContextAccessor>();
            var userPrincipalFactory = new Mock<IUserClaimsPrincipalFactory<User>>();
            _mockSignInManager = new Mock<SignInManager<User>>(_mockUserManager.Object, contextAccessor.Object, userPrincipalFactory.Object, null!, null!, null!, null!);

            _mockLogger = new Mock<ILogger<AccountController>>();

            _controller = new AccountController(_mockSignInManager.Object, _mockUserManager.Object, _mockLogger.Object);
        }

        [Fact]
        public async Task Login_CorrectCredentials_SignsInUser()
        {
            string validEmail = "test@test.com";
            string validPassword = "ValidPassword123!";

            var model = new LoginViewModel { Email = validEmail, Password = validPassword };
            var user = new User { UserName = validEmail, Email = validEmail };

            _mockUserManager.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _mockSignInManager.Setup(x => x.PasswordSignInAsync(user.UserName, model.Password, false, false))
                              .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var result = await _controller.Login(model);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
            Assert.Equal("Home", redirectToActionResult.ControllerName);
        }

        [Fact]
        public async Task Login_WrongPassword_ReturnsInvalidAttempt()
        {
            string validEmail = "test@test.com";
            string wrongPassword = "WrongPassword";

            var model = new LoginViewModel { Email = validEmail, Password = wrongPassword };
            var user = new User { UserName = validEmail, Email = validEmail };

            _mockUserManager.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync(user);
            _mockSignInManager.Setup(x => x.PasswordSignInAsync(user.UserName, model.Password, false, false))
                              .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var result = await _controller.Login(model);

            Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ErrorCount > 0);
        }

        [Fact]
        public async Task Login_UnregisteredEmail_ReturnsError()
        {
            string unregisteredEmail = "notfound@test.com";
            string anyPassword = "Password123!";

            var model = new LoginViewModel { Email = unregisteredEmail, Password = anyPassword };

            _mockUserManager.Setup(x => x.FindByEmailAsync(model.Email)).ReturnsAsync((User?)null);

            var result = await _controller.Login(model);

            Assert.IsType<ViewResult>(result);
            Assert.False(_controller.ModelState.IsValid);
            Assert.True(_controller.ModelState.ErrorCount > 0);
        }
    }
}