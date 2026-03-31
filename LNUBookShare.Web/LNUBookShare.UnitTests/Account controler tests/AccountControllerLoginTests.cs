using LNUBookShare.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LNUBookShare.UnitTests.AccountControllerTests;

public class AccountControllerLoginTests : AccountControllerTestBase
{
    [Fact]
    public async Task ConfirmEmail_Successful_RedirectsToLogin()
    {
        _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(new User { Id = 1 });
        _userManagerMock.Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);

        var result = await _controller.ConfirmEmail("1", "token");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);
    }

    [Fact]
    public async Task ConfirmEmail_UserNotFound_ReturnsNotFound()
    {
        // Arrange
        _userManagerMock.Setup(x => x.FindByIdAsync("999")).ReturnsAsync((User)null!);

        // Act
        var result = await _controller.ConfirmEmail("999", "token");

        // Assert
        // Перевіряємо, що повернувся саме NotFoundObjectResult
        var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
        Assert.Equal("Користувача не знайдено.", notFoundResult.Value);
    }
}

