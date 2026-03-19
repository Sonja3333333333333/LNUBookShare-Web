using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;

namespace LNUBookShare.UnitTests.AccountControllerTests; // Зверни увагу на неймспейс (через крапку додалася папка)

public abstract class AccountControllerTestBase
{
    // protected означає, що ці змінні будуть видимі у класах-спадкоємцях (LoginTests, RegisterTests)
    protected readonly Mock<UserManager<User>> _userManagerMock;
    protected readonly Mock<SignInManager<User>> _signInManagerMock;
    protected readonly Mock<ILogger<AccountController>> _loggerMock;
    protected readonly Mock<IFacultyRepository> _facultyRepoMock;
    protected readonly Mock<IEmailService> _emailServiceMock;
    protected readonly AccountController _controller;

    protected AccountControllerTestBase()
    {
        // 1. Створюємо "фейкові" сервіси
        _userManagerMock = new Mock<UserManager<User>>(
            new Mock<IUserStore<User>>().Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _signInManagerMock = new Mock<SignInManager<User>>(
            _userManagerMock.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            null!, null!, null!, null!);

        _loggerMock = new Mock<ILogger<AccountController>>();
        _facultyRepoMock = new Mock<IFacultyRepository>();
        _emailServiceMock = new Mock<IEmailService>();

        // 2. Створюємо контролер, передаючи йому ці фейки
        _controller = new AccountController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _loggerMock.Object,
            _facultyRepoMock.Object,
            _emailServiceMock.Object);

        // 3. "Пришиваємо" органи контролеру (HttpContext, TempData, Url)
        // Це робиться один раз тут, і працюватиме для всіх тестів
        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("http://fake-link");
        _controller.Url = urlHelperMock.Object;
    }
}