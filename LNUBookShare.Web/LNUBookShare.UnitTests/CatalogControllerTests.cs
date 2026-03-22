using System.Security.Claims;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Controllers;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests
{
    public class CatalogControllerTests
    {
        private readonly Mock<IBookSearchService> _searchServiceMock;
        private readonly Mock<ILogger<CatalogController>> _loggerMock;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly CatalogController _controller;
        private readonly Mock<IFavoriteService> _favoriteService;

        public CatalogControllerTests()
        {
            _searchServiceMock = new Mock<IBookSearchService>();
            _loggerMock = new Mock<ILogger<CatalogController>>();
            _favoriteService = new Mock<IFavoriteService>();

            // Складний мок для UserManager (стандарт для Identity)
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _controller = new CatalogController(_searchServiceMock.Object, _loggerMock.Object, _userManagerMock.Object, _favoriteService.Object);

            // --- Імітація залогіненого користувача ---
            // Це потрібно, щоб контролер міг "побачити" поточного юзера через User (ClaimsPrincipal)
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "mock"));

            _controller.ControllerContext = new ControllerContext()
            {
                HttpContext = new DefaultHttpContext() { User = user }
            };
        }

        [Fact]
        public async Task Search_WhenQueryIsEmpty_ShouldReturnRecommendationsForUser()
        {
            // Arrange
            var testUser = new User { Id = 1, FacultyId = 3 }; // Юзер із факультету №3
            var recommendedBooks = new List<Book> { new Book { Title = "Рекомендована книга" } };

            // Кажемо UserManager повернути нашого юзера
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(testUser);

            // Кажемо Сервісу повернути список рекомендацій для цього факультету
            _searchServiceMock.Setup(s => s.GetRecommendationsAsync(3, 1, "title", "all"))
                              .ReturnsAsync(recommendedBooks);

            // Act
            // Викликаємо Search з порожнім запитом (імітуємо вхід на головну сторінку каталогу)
            var result = await _controller.Search(null!);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookSearchViewModel>(viewResult.Model);

            // 1. Перевіряємо, чи в моделі саме ті книги, які дав сервіс рекомендацій
            Assert.Equal(recommendedBooks.Count, model.Books.Count());

            // 2. Перевіряємо, чи контролер поставив прапорець рекомендацій у ViewBag
            Assert.True(_controller.ViewBag.IsRecommendation);

            // 3. Перевіряємо, чи викликався саме метод GetRecommendationsAsync, а не звичайний Search
            _searchServiceMock.Verify(s => s.GetRecommendationsAsync(3, 1, "title", "all"), Times.Once);

            // 4. Перевіряємо, чи був запис у лог Seq про генерацію рекомендацій
            _loggerMock.Verify(
                x => x.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Згенеровано рекомендації")),
                    null,
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }
    }
}