using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;


namespace LNUBookShare.UnitTests.OtherProfileService_tests
{
    public class OtherProfileTests
    {
        private readonly Mock<IOtherProfileRepository> _otherProfileRepoMock;
        private readonly Mock<ILogger<OtherProfileService>> _loggerMock;
        private readonly OtherProfileService _otherProfileService;

        public OtherProfileTests()
        {
            _otherProfileRepoMock = new Mock<IOtherProfileRepository>();
            _loggerMock = new Mock<ILogger<OtherProfileService>>();
            _otherProfileService = new OtherProfileService(_otherProfileRepoMock.Object, _loggerMock.Object);
        }

        [Fact]
        public async Task GetOtherUser_WhenUserExists_ShouldReturnSuccessWithUser()
        {
            // Arrange
            int expectedUserId = 1;
            var expectedUser = new User
            {
                Id = expectedUserId,
                FirstName = "Іван",
                LastName = "Андріїшин",
                Email = "Ivan.Andriishun@lnu.edu.ua",
                Faculty = new Faculty { FacultyName = "Прикладної математики та інформатики" },
                Avatar = new Image { ImagePath = "/images/avatars/default.png" }
            };

            _otherProfileRepoMock.Setup(repo => repo.GetUserById(expectedUserId))
                .ReturnsAsync(expectedUser);

            //Act
            var result = await _otherProfileService.GetOtherUser(expectedUserId);


            //Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);

            var res = result.Value;
            Assert.Equal(expectedUser.Id, res.Id);
            Assert.Equal(expectedUser.Email, res.Email);
            Assert.Equal(expectedUser.Faculty.FacultyName, res.Faculty.FacultyName);
            Assert.Equal(expectedUser.Avatar!.ImagePath, res.Avatar!.ImagePath);
        }

        [Fact]
        public async Task GetOtherUser_WhenUserDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            int nonExistentUserId = 1000;

            _otherProfileRepoMock.Setup(repo => repo.GetUserById(nonExistentUserId))
                .ReturnsAsync((User?)null);

            // Act
            var result = await _otherProfileService.GetOtherUser(nonExistentUserId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Користувача не знайдено.", result.Error);

            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Warning,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("не існує")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task GetOtherUserBooks_WhenCalled_ShouldReturnSuccessWithBooks()
        {
            // Arrange
            int userId = 1;
            string sortBy = "author";
            string statusFilter = "available";
            var expectedBooks = new List<Book> { 
                new Book { BookId = 1, Title = "Тестова книга 1" },
                new Book { BookId = 2, Title = "Тестова книга 2" }};

            _otherProfileRepoMock.Setup(repo => repo.GetUserBooks(userId, sortBy, statusFilter))
                .ReturnsAsync(expectedBooks);

            // Act
            var result = await _otherProfileService.GetOtherUserBooks(userId, sortBy, statusFilter);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.Equal(expectedBooks.Count, result.Value.Count());
                        
            _otherProfileRepoMock.Verify(repo => repo.GetUserBooks(userId, sortBy, statusFilter), Times.Once);
        }
    }
}
        