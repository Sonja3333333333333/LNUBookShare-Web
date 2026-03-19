using LNUBookShare.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace LNUBookShare.UnitTests.AccountControllerTests;

// Тепер цей клас чистий, як сльоза. Тільки тести!
public class AccountControllerLoginTests : AccountControllerTestBase
{
    [Fact]
    public async Task ConfirmEmail_Successful_RedirectsToLogin()
    {
        // _userManagerMock вже доступний тут, бо ми успадкували базу!
        _userManagerMock.Setup(x => x.FindByIdAsync("1")).ReturnsAsync(new User { Id = 1 });
        _userManagerMock.Setup(x => x.ConfirmEmailAsync(It.IsAny<User>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);

        var result = await _controller.ConfirmEmail("1", "token");

        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirect.ActionName);
    }
}