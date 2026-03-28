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

            _profileService = new ProfileService(_profileRepositoryMock.Object, _loggerMock.Object);
        }

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
                Avatar = new Image { ImagePath = "/images/avatars/default.png" }
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

    }

}
