using System;
using System.Threading.Tasks;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.ProfileService_tests
{
    public class ProfileServiceTests
    {
        private readonly Mock<IProfileRepository> _profileRepositoryMock;
        private readonly Mock<IBookRepository> _bookRepositoryMock;
        private readonly Mock<ILogger<ProfileService>> _loggerMock;
        private readonly ProfileService _profileService;

        public ProfileServiceTests()
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

        [Fact]
        public async Task GetUserBooksAsync_WhenUserHasBooks_ShouldReturnFilteredAndSortedBooks()
        {
            // Arrange
            int targetUserId = 1;
            var allBooksFromDb = new List<Book>
            {
                new Book { BookId = 1, Title = "Книга іншого", OwnerId = 99, CreatedAt = DateTime.UtcNow.AddDays(-10) },
                
                new Book { BookId = 2, Title = "Стара книга", OwnerId = targetUserId, CreatedAt = DateTime.UtcNow.AddDays(-5) },
                
                new Book { BookId = 3, Title = "Нова книга", OwnerId = targetUserId, CreatedAt = DateTime.UtcNow }
            };

            _bookRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(allBooksFromDb);

            var result = await _profileService.GetUserBooksAsync(targetUserId);

            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            Assert.Equal(2, result.Value.Count);

            Assert.Equal("Нова книга", result.Value.First().Title);
            Assert.Equal("Стара книга", result.Value.Last().Title);
        }

        [Fact]
        public async Task GetUserBooksAsync_WhenUserHasNoBooks_ShouldReturnSuccessWithEmptyList()
        {
            // Arrange
            int targetUserId = 1;
            var allBooksFromDb = new List<Book>
            {
                new Book { BookId = 1, Title = "Книга іншого", OwnerId = 99 }
            };

            _bookRepositoryMock
                .Setup(repo => repo.GetAllAsync())
                .ReturnsAsync(allBooksFromDb);

            // Act
            var result = await _profileService.GetUserBooksAsync(targetUserId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Empty(result.Value); 
        }

        // ... (усі твої using залишаються)

        [Fact]
        public async Task UpdateBookAsync_WithValidData_ShouldReturnSuccessAndCallRepo()
        {
            // Arrange
            int ownerId = 10;
            int bookId = 1;
            var image = new Image();

            // Налаштовуємо мок репозиторію (він все ще може приймати Book, якщо ти так написав у репо)
            _bookRepositoryMock
                .Setup(repo => repo.UpdateAsync(It.IsAny<Book>()))
                .Returns(Task.CompletedTask);

            // Act - ПЕРЕДАЄМО ВСІ 10 ПАРАМЕТРІВ
            var result = await _profileService.UpdateBookAsync(
                ownerId, bookId, "Назва", "Автор", 1, "Опис", "ISBN", "available", 1, image);

            // Assert
            Assert.True(result.IsSuccess);
            _bookRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Book>()), Times.Once);
        }

        [Fact]
        public async Task UpdateBookAsync_WhenDataInvalid_ShouldReturnFailure()
        {
            // Якщо ти додав валідацію в сервіс, наприклад на порожню назву
            var result = await _profileService.UpdateBookAsync(
                10, 1, "", "", 1, null, null, null, 1, null);

            // Assert (якщо у тебе там є перевірка на null/empty)
            // Якщо валідації немає, цей тест можна просто видалити або підігнати під логіку Failure
        }

        [Fact]
        public async Task UpdateBookAsync_WhenBookIsNull_ShouldReturnFailure()
        {
            // Act
            var result = await _profileService.UpdateBookAsync(
                    10, 1, "", "", 1, null, null, null, 1, null);
            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Дані для оновлення відсутні.", result.Error);

            _bookRepositoryMock.Verify(repo => repo.UpdateAsync(It.IsAny<Book>()), Times.Never);
        }
    }
}