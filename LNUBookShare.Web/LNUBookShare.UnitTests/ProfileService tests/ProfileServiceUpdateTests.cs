using System.Threading.Tasks;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.ProfileService_tests
{
    public class ProfileServiceUpdateTests
    {
        private readonly Mock<IProfileRepository> _profileRepositoryMock;
        private readonly Mock<IBookRepository> _bookRepositoryMock;
        private readonly Mock<ILogger<ProfileService>> _loggerMock;
        private readonly ProfileService _profileService;

        public ProfileServiceUpdateTests()
        {
            _profileRepositoryMock = new Mock<IProfileRepository>();
            _bookRepositoryMock = new Mock<IBookRepository>();
            _loggerMock = new Mock<ILogger<ProfileService>>();

            _profileService = new ProfileService(
                _profileRepositoryMock.Object,
                _bookRepositoryMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenUserExists_ShouldReturnSuccess()
        {
            // Arrange
            var user = new User { Id = 1, FirstName = "Старе", LastName = "Ім'я", FacultyId = 1 };

            _profileRepositoryMock
                .Setup(r => r.GetUserByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            var result = await _profileService.UpdateProfileAsync(1, "Нове", "Прізвище", 2, null);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Нове", user.FirstName);
            Assert.Equal("Прізвище", user.LastName);
            Assert.Equal(2, user.FacultyId);

            _profileRepositoryMock.Verify(r => r.UpdateUserAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenUserNotFound_ShouldReturnFailure()
        {
            // Arrange
            _profileRepositoryMock
                .Setup(r => r.GetUserByIdAsync(999))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _profileService.UpdateProfileAsync(999, "Ім'я", "Прізвище", 1, null);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Користувача не знайдено.", result.Error);

            _profileRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenAvatarPathProvided_ShouldSetAvatar()
        {
            // Arrange
            var user = new User { Id = 1, FirstName = "Іван", LastName = "Франко", FacultyId = 1 };

            _profileRepositoryMock
                .Setup(r => r.GetUserByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            var result = await _profileService.UpdateProfileAsync(1, "Іван", "Франко", 1, "https://cloudinary.com/avatar.jpg");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(user.Avatar);
            Assert.Equal("https://cloudinary.com/avatar.jpg", user.Avatar!.ImagePath);
            Assert.Equal("Avatar", user.Avatar.ImageType);
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenAvatarPathIsNull_ShouldNotChangeAvatar()
        {
            // Arrange
            var existingAvatar = new Image { ImagePath = "old_avatar.jpg", ImageType = "Avatar" };
            var user = new User { Id = 1, FirstName = "Іван", LastName = "Франко", FacultyId = 1, Avatar = existingAvatar };

            _profileRepositoryMock
                .Setup(r => r.GetUserByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            var result = await _profileService.UpdateProfileAsync(1, "Іван", "Франко", 1, null);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("old_avatar.jpg", user.Avatar?.ImagePath);
        }

        [Fact]
        public async Task UpdateProfileAsync_ShouldCallUpdateUserAsync_Once()
        {
            // Arrange
            var user = new User { Id = 1, FirstName = "Тест", LastName = "Юзер", FacultyId = 1 };

            _profileRepositoryMock
                .Setup(r => r.GetUserByIdAsync(1))
                .ReturnsAsync(user);

            // Act
            await _profileService.UpdateProfileAsync(1, "Новий", "Юзер", 3, null);

            // Assert
            _profileRepositoryMock.Verify(r => r.UpdateUserAsync(It.IsAny<User>()), Times.Once);
        }
    }
}