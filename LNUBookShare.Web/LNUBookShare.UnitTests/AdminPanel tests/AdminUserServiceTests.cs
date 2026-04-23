using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Models; // Перевір, чи UserDto тут
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.Services
{
    public class AdminUserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ILogger<AdminUserService>> _loggerMock;
        private readonly AdminUserService _service;

        public AdminUserServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<AdminUserService>>();

            _service = new AdminUserService(_userRepoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetAllUsersAsync_DefaultBehavior_ReturnsMappedDtos()
        {
            // Сценарій 1: Дефолтна поведінка (Успішне отримання списку)
            // Arrange
            var mockUsers = new List<User>
            {
                new User {
                    Id = 1,
                    FirstName = "Maksym",
                    LastName = "Bohdanovych",
                    Email = "max@lnu.edu.ua",
                    Faculty = new Faculty { FacultyName = "ПМІ" }
                }
            };
            _userRepoMock.Setup(r => r.GetUsersWithDetailsAsync(null))
                .ReturnsAsync(mockUsers);

            // Act
            var result = await _service.GetAllUsersAsync();

            // Assert
            Assert.True(result.IsSuccess);
            var dtos = result.Value.ToList();
            Assert.Single(dtos);
            Assert.Equal("Maksym Bohdanovych", dtos[0].FullName);
            Assert.Equal("ПМІ", dtos[0].FacultyName);
        }

        [Fact]
        public async Task GetAllUsersAsync_WithSearchTerm_CallsRepoWithCorrectFilter()
        {
            // Сценарій 2: Робота з пошуком
            // Arrange
            string searchTerm = "Ivan";
            _userRepoMock.Setup(r => r.GetUsersWithDetailsAsync(searchTerm))
                .ReturnsAsync(new List<User>());

            // Act
            await _service.GetAllUsersAsync(searchTerm);

            // Assert
            // Перевіряємо, чи ми передали слово "Ivan" саме в репозиторій
            _userRepoMock.Verify(r => r.GetUsersWithDetailsAsync("Ivan"), Times.Once);
        }

        [Fact]
        public async Task GetAllUsersAsync_WhenExceptionOccurs_ReturnsFailureAndLogsError()
        {
            // Сценарій 3: Виключна ситуація (БД впала)
            // Arrange
            _userRepoMock.Setup(r => r.GetUsersWithDetailsAsync(It.IsAny<string>()))
                .ThrowsAsync(new System.Exception("Neon is offline"));

            // Act
            var result = await _service.GetAllUsersAsync();

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Помилка при завантаженні даних.", result.Error);

            // Перевіряємо, чи спрацював логер
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }
    }
}