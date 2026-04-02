using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LNUBookShare.UnitTests.ProfileService_tests
{
    public class ProfileServiceTests
    {
        private readonly Mock<IProfileRepository> _profileRepositoryMock;
        private readonly Mock<ILogger<ProfileService>> _loggerMock;
        private readonly ProfileService _profileService;

        public ProfileServiceTests()
        {
            _profileRepositoryMock = new Mock<IProfileRepository>();
            _loggerMock = new Mock<ILogger<ProfileService>>();

            _profileService = new ProfileService(
                _profileRepositoryMock.Object,
                _loggerMock.Object);
        }

        // ========================
        // GetUserProfileAsync
        // ========================

        [Fact]
        public async Task GetUserProfileAsync_WhenUserExists_ShouldReturnSuccessWithDto()
        {
            // Arrange
            int expectedUserId = 1;
            var fakeUser = new User
            {
                Id = expectedUserId,
                FirstName = "Софія",
                LastName = "Богданович",
                Email = "SOFIIA.BOHDANOVYCH@lnu.edu.ua",
                Faculty = new Faculty { FacultyName = "Прикладної математики та інформатики" },
                Avatar = new Image { ImagePath = "/images/avatars/default.png" },
            };

            _profileRepositoryMock
                .Setup(repo => repo.GetUserDetailsAsync(expectedUserId))
                .ReturnsAsync(fakeUser);

            // Act
            var result = await _profileService.GetUserProfileAsync(expectedUserId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var dto = result.Value;
            Assert.Equal(fakeUser.Id, dto.UserId);
            Assert.Equal(fakeUser.FirstName, dto.FirstName);
            Assert.Equal(fakeUser.LastName, dto.LastName);
            Assert.Equal(fakeUser.Email, dto.Email);
            Assert.Equal(fakeUser.Faculty.FacultyName, dto.FacultyName);
            Assert.Equal(fakeUser.Avatar.ImagePath, dto.AvatarPath);
        }

        [Fact]
        public async Task GetUserProfileAsync_WhenUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            int nonExistentUserId = 999;

            _profileRepositoryMock
                .Setup(repo => repo.GetUserDetailsAsync(nonExistentUserId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _profileService.GetUserProfileAsync(nonExistentUserId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Користувача не знайдено.", result.Error);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Спроба перегляду профілю для неіснуючого користувача")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        // ========================
        // UpdateProfileAsync
        // ========================

        [Fact]
        public async Task UpdateProfileAsync_WhenUserExists_ShouldUpdateFieldsAndReturnSuccess()
        {
            // Arrange
            var fakeUser = new User
            {
                Id = 1,
                FirstName = "Старе",
                LastName = "Ім'я",
                FacultyId = 1,
            };

            _profileRepositoryMock
                .Setup(repo => repo.GetUserDetailsAsync(1))
                .ReturnsAsync(fakeUser);

            // Act
            var result = await _profileService.UpdateProfileAsync(
                userId: 1,
                firstName: "Нове",
                lastName: "Прізвище",
                facultyId: 2,
                newAvatarPath: null);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("Нове", fakeUser.FirstName);
            Assert.Equal("Прізвище", fakeUser.LastName);
            Assert.Equal(2, fakeUser.FacultyId);

            _profileRepositoryMock.Verify(
                repo => repo.UpdateUserAsync(fakeUser),
                Times.Once);
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            _profileRepositoryMock
                .Setup(repo => repo.GetUserDetailsAsync(999))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _profileService.UpdateProfileAsync(
                userId: 999,
                firstName: "Нове",
                lastName: "Прізвище",
                facultyId: 1,
                newAvatarPath: null);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Користувача не знайдено.", result.Error);

            _profileRepositoryMock.Verify(
                repo => repo.UpdateUserAsync(It.IsAny<User>()),
                Times.Never);
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenNewAvatarProvided_ShouldSetAvatarPath()
        {
            // Arrange
            var fakeUser = new User
            {
                Id = 1,
                FirstName = "Тест",
                LastName = "Юзер",
                FacultyId = 1,
                Avatar = null,
            };

            _profileRepositoryMock
                .Setup(repo => repo.GetUserDetailsAsync(1))
                .ReturnsAsync(fakeUser);

            // Act
            var result = await _profileService.UpdateProfileAsync(
                userId: 1,
                firstName: "Тест",
                lastName: "Юзер",
                facultyId: 1,
                newAvatarPath: "/images/avatars/new.jpg");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(fakeUser.Avatar);
            Assert.Equal("/images/avatars/new.jpg", fakeUser.Avatar.ImagePath);
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenAvatarExistsAndNewProvided_ShouldUpdateAvatarPath()
        {
            // Arrange
            var fakeUser = new User
            {
                Id = 1,
                FirstName = "Тест",
                LastName = "Юзер",
                FacultyId = 1,
                Avatar = new Image
                {
                    ImagePath = "/images/avatars/old.jpg",
                    ImageType = "avatar",
                },
            };

            _profileRepositoryMock
                .Setup(repo => repo.GetUserDetailsAsync(1))
                .ReturnsAsync(fakeUser);

            // Act
            var result = await _profileService.UpdateProfileAsync(
                userId: 1,
                firstName: "Тест",
                lastName: "Юзер",
                facultyId: 1,
                newAvatarPath: "/images/avatars/new.jpg");

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("/images/avatars/new.jpg", fakeUser.Avatar.ImagePath);
        }

        [Fact]
        public async Task UpdateProfileAsync_WhenNoNewAvatar_ShouldNotChangeExistingAvatar()
        {
            // Arrange
            var fakeUser = new User
            {
                Id = 1,
                FirstName = "Тест",
                LastName = "Юзер",
                FacultyId = 1,
                Avatar = new Image
                {
                    ImagePath = "/images/avatars/existing.jpg",
                    ImageType = "avatar",
                },
            };

            _profileRepositoryMock
                .Setup(repo => repo.GetUserDetailsAsync(1))
                .ReturnsAsync(fakeUser);

            // Act
            var result = await _profileService.UpdateProfileAsync(
                userId: 1,
                firstName: "Тест",
                lastName: "Юзер",
                facultyId: 1,
                newAvatarPath: null);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal("/images/avatars/existing.jpg", fakeUser.Avatar.ImagePath);
        }
    }
}
