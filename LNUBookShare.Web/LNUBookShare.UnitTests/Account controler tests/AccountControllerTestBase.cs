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

namespace LNUBookShare.UnitTests.AccountControllerTests;

public abstract class AccountControllerTestBase
{
    protected readonly Mock<UserManager<User>> _userManagerMock;
    protected readonly Mock<SignInManager<User>> _signInManagerMock;
    protected readonly Mock<ILogger<AccountController>> _loggerMock;

    protected readonly Mock<IFacultyService> _facultyServiceMock;
    protected readonly Mock<IEmailService> _emailServiceMock;
    protected readonly AccountController _controller;

    protected AccountControllerTestBase()
    {
        _userManagerMock = new Mock<UserManager<User>>(
            new Mock<IUserStore<User>>().Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _signInManagerMock = new Mock<SignInManager<User>>(
            _userManagerMock.Object,
            new Mock<IHttpContextAccessor>().Object,
            new Mock<IUserClaimsPrincipalFactory<User>>().Object,
            null!, null!, null!, null!);

        _loggerMock = new Mock<ILogger<AccountController>>();

        _facultyServiceMock = new Mock<IFacultyService>();
        _emailServiceMock = new Mock<IEmailService>();

        _controller = new AccountController(
            _userManagerMock.Object,
            _signInManagerMock.Object,
            _loggerMock.Object,
            _facultyServiceMock.Object,
            _emailServiceMock.Object);

        var httpContext = new DefaultHttpContext();
        _controller.ControllerContext = new ControllerContext { HttpContext = httpContext };
        _controller.TempData = new TempDataDictionary(httpContext, Mock.Of<ITempDataProvider>());

        var urlHelperMock = new Mock<IUrlHelper>();
        urlHelperMock.Setup(x => x.Action(It.IsAny<UrlActionContext>())).Returns("http://fake-link");
        _controller.Url = urlHelperMock.Object;
    }
}