using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.AdminUserService_tests
{
    public class AdminUserServiceTests
    {
        private readonly Mock<IUserRepository> _userRepoMock;
        private readonly Mock<ILogger<AdminUserService>> _loggerMock;
        private readonly AdminUserService _adminUserService;

        public AdminUserServiceTests()
        {
            _userRepoMock = new Mock<IUserRepository>();
            _loggerMock = new Mock<ILogger<AdminUserService>>();

            _adminUserService = new AdminUserService(
                _userRepoMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task BlockUserAsync_WhenUserIsActive_ShouldBlockAndReturnSuccess()
        {
            int userId = 1;
            var user = new User { Id = userId, IsActive = true };
            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            var result = await _adminUserService.BlockUserAsync(userId);

            Assert.True(result.IsSuccess);
            Assert.False(user.IsActive); 
            _userRepoMock.Verify(repo => repo.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task BlockUserAsync_WhenUserIsAlreadyBlocked_ShouldReturnFailure()
        {
            int userId = 1;
            var user = new User { Id = userId, IsActive = false }; 
            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            var result = await _adminUserService.BlockUserAsync(userId);

            Assert.True(result.IsFailure);
            Assert.Equal("Користувач вже заблокований.", result.Error);
            _userRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task BlockUserAsync_WhenUserDoesNotExist_ShouldReturnFailure()
        {
            int userId = 99;
            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync((User?)null);

            var result = await _adminUserService.BlockUserAsync(userId);

            Assert.True(result.IsFailure);
            Assert.Equal("Користувача не знайдено.", result.Error);
            _userRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UnblockUserAsync_WhenUserIsBlocked_ShouldUnblockAndReturnSuccess()
        {
            int userId = 1;
            var user = new User { Id = userId, IsActive = false };
            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            var result = await _adminUserService.UnblockUserAsync(userId);

            Assert.True(result.IsSuccess);
            Assert.True(user.IsActive);
            _userRepoMock.Verify(repo => repo.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UnblockUserAsync_WhenUserIsAlreadyActive_ShouldReturnFailure()
        {
            int userId = 1;
            var user = new User { Id = userId, IsActive = true };
            _userRepoMock.Setup(repo => repo.GetByIdAsync(userId)).ReturnsAsync(user);

            var result = await _adminUserService.UnblockUserAsync(userId);

            Assert.True(result.IsFailure);
            Assert.Equal("Користувач вже розблокований (активний).", result.Error);
            _userRepoMock.Verify(repo => repo.UpdateAsync(It.IsAny<User>()), Times.Never);
        }
    }
}