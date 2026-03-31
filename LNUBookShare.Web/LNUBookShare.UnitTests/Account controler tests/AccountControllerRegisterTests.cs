using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.AccountControllerTests;

// Успадковуємо AccountControllerTestBase. 
// Весь конструктор і моки автоматично "прилетіли" сюди.
public class AccountControllerRegisterTests : AccountControllerTestBase
{
    [Fact]
    public async Task Register_InvalidEmailDomain_LogsWarningAndReturnsView()
    {
        // Arrange
        var model = new RegisterViewModel { Email = "hacker@gmail.com", Password = "Password123" };
        _facultyRepoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Faculty>());

        // Act
        var result = await _controller.Register(model);

        // Assert
        Assert.IsType<ViewResult>(result);
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Спроба реєстрації з лівого домену")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
        
    [Fact]
    public async Task Register_SuccessfulRegistration_LogsInformationAndRedirects()
    {
        // Arrange
        var model = new RegisterViewModel
        {
            Email = "student@lnu.edu.ua",
            Password = "Password123",
            FirstName = "Ivan",
            LastName = "Ivanov",
            FacultyId = 1
        };

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);

        _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                        .ReturnsAsync("fake-token-123");

        // Act
        var result = await _controller.Register(model);

        // Assert
        var redirectResult = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Login", redirectResult.ActionName);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Успішна реєстрація в БД")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task Register_EmailServiceFails_LogsError()
    {
        // Arrange
        var model = new RegisterViewModel { Email = "test@lnu.edu.ua", Password = "Password123" };

        _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                        .ReturnsAsync(IdentityResult.Success);
        _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<User>()))
                        .ReturnsAsync("token");

        _emailServiceMock.Setup(x => x.SendEmailAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
                         .ThrowsAsync(new System.Exception("SMTP Server Down"));

        // Act
        await _controller.Register(model);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Помилка при відправці пошти")),
                It.IsAny<System.Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}