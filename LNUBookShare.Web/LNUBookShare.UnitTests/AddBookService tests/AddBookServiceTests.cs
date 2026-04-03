using Moq;
using Xunit; 
using System.Threading.Tasks; 
using LNUBookShare.Application.Services;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.UnitTests.Services
{
    public class AddBookServiceTests
    {
        private readonly Mock<IBookRepository> _bookRepoMock;
        private readonly Mock<IProfileRepository> _profileRepoMock;
        private readonly Mock<ILogger<ProfileService>> _loggerMock;
        private readonly ProfileService _profileService;

        public AddBookServiceTests()
        {
            _bookRepoMock = new Mock<IBookRepository>();
            _profileRepoMock = new Mock<IProfileRepository>();
            _loggerMock = new Mock<ILogger<ProfileService>>();

            _profileService = new ProfileService(
                _profileRepoMock.Object,
                _bookRepoMock.Object,
                _loggerMock.Object);
        }

        [Fact]
        public async Task AddBookToProfileAsync_ShouldReturnSuccess_WhenDataIsValid()
        {
            var validBook = new Book
            {
                Title = "Кобзар",
                Author = "Тарас Шевченко",
                OwnerId = 1
            };

            var result = await _profileService.AddBookToProfileAsync(validBook);

            Assert.True(result.IsSuccess);
            _bookRepoMock.Verify(r => r.AddAsync(It.IsAny<Book>()), Times.Once);
        }
    }
}