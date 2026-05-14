using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.Services
{
    public class ProfileServiceTests
    {
        private readonly Mock<IProfileRepository> _profileRepoMock;
        private readonly Mock<IBookRepository> _bookRepoMock;
        private readonly Mock<ILogger<ProfileService>> _loggerMock;
        private readonly ProfileService _service;

        public ProfileServiceTests()
        {
            _profileRepoMock = new Mock<IProfileRepository>();
            _bookRepoMock = new Mock<IBookRepository>();
            _loggerMock = new Mock<ILogger<ProfileService>>();

            _service = new ProfileService(
                _profileRepoMock.Object,
                _bookRepoMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task GetUserProfileAsync_WhenUserExists_ReturnsSuccess()
        {
            int userId = 1;
            var user = new User { Id = userId, FirstName = "Максим", Faculty = new Faculty { FacultyName = "ПМІ" } };
            _profileRepoMock.Setup(r => r.GetUserDetailsAsync(userId)).ReturnsAsync(user);

            var result = await _service.GetUserProfileAsync(userId);

            Assert.True(result.IsSuccess);
            Assert.Equal("Максим", result.Value.FirstName);
        }

        [Fact]
        public async Task GetUserProfileAsync_WhenUserNotFound_ReturnsFailure()
        {
            // ФІКС CS8600: додаємо знак питання до User
            _profileRepoMock.Setup(r => r.GetUserDetailsAsync(It.IsAny<int>())).ReturnsAsync((User?)null);

            var result = await _service.GetUserProfileAsync(999);

            Assert.True(result.IsFailure);
            Assert.Equal("Користувача не знайдено.", result.Error);
        }

        [Fact]
        public async Task UpdateBookAsync_WithValidData_ShouldReturnSuccess()
        {
            int bookId = 1, ownerId = 10;
            var book = new Book { BookId = bookId, OwnerId = ownerId };

            // Обов'язково налаштовуємо GetByIdAsync, інакше сервіс поверне Failure
            _bookRepoMock.Setup(r => r.GetByIdAsync(bookId)).ReturnsAsync(book);
            _bookRepoMock.Setup(r => r.UpdateAsync(It.IsAny<Book>())).Returns(Task.CompletedTask);

            var result = await _service.UpdateBookAsync(bookId, ownerId, "New Title", "Author", 2024, null, null, null, 1, null);

            Assert.True(result.IsSuccess);
            _bookRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Book>()), Times.Once);
        }
    }
}