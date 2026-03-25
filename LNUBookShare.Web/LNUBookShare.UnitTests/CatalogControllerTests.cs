using System.Security.Claims;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Web.Controllers;
using LNUBookShare.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LNUBookShare.Application.Common;
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
        private readonly Mock<IBookDetailsService> _bookDetailsServiceMock;

        public CatalogControllerTests()
        {
            _searchServiceMock = new Mock<IBookSearchService>();
            _loggerMock = new Mock<ILogger<CatalogController>>();
            _favoriteService = new Mock<IFavoriteService>();
            _bookDetailsServiceMock = new Mock<IBookDetailsService>();

            // Складний мок для UserManager (стандарт для Identity)
            var userStoreMock = new Mock<IUserStore<User>>();
            _userManagerMock = new Mock<UserManager<User>>(userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _controller = new CatalogController(_searchServiceMock.Object, _loggerMock.Object, _userManagerMock.Object, _favoriteService.Object, _bookDetailsServiceMock.Object);

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

            _favoriteService.Setup(f => f.GetUserFavoriteBookIdsAsync(testUser.Id))
                .ReturnsAsync(Result<IEnumerable<int>>.Success(new List<int>()));

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

        [Fact]
        public async Task Details_WhenBookExists_ShouldReturnViewWithCorrectViewModel()
        {
            // Arrange
            int bookId = 1;
            string? returnUrl = "/Catalog/Search?query=test";

            var bookFromService = new Book
            {
                BookId = bookId,
                Title = "Clean Architecture",
                Author = "Robert Martin",
                Owner = new User { FirstName = "Ivan" },
                Cover = new Image { ImagePath = "cover.jpg" }
            };

            
            _bookDetailsServiceMock
                .Setup(s => s.GetBookDetailsAsync(bookId))
                .ReturnsAsync(Result<Book>.Success(bookFromService));

            // Налаштовуємо UserManager для приватного методу GetUserFavoriteIdsAsync
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>()))
                            .ReturnsAsync(new User { Id = 1 });

            _favoriteService.Setup(f => f.GetUserFavoriteBookIdsAsync(It.IsAny<int>()))
                            .ReturnsAsync(Result<IEnumerable<int>>.Success(new List<int> { bookId }));

            // Act
            var result = await _controller.Details(bookId, returnUrl);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<BookDetailsViewModel>(viewResult.Model);

            
            Assert.Equal(bookFromService.Title, model.Title);
            Assert.Equal(bookFromService.Owner.FirstName, model.Owner?.FirstName);
            Assert.Equal(bookFromService.Cover.ImagePath, model.Cover?.ImagePath);
            Assert.Equal(bookId, model.BookId);

            
            Assert.Equal(returnUrl, _controller.ViewBag.ReturnUrl);

            // Перевіряємо, чи підтягнулися вподобання
            Assert.Contains(bookId, model.FavoritedBookIds);
        }


        [Fact]
        public async Task Details_WhenBookDoesNotExist_ShouldReturnNotFoundWithErrorMessage()
        {
            // Arrange
            int nonExistentId = 1000;
            string expectedErrorMessage = "Книга не знайдена";

            _bookDetailsServiceMock
                .Setup(s => s.GetBookDetailsAsync(nonExistentId))
                .ReturnsAsync(Result<Book>.Failure(expectedErrorMessage));

            // Act
            var result = await _controller.Details(nonExistentId);

            // Assert            
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);

            // Перевіряємо, чи текст помилки відповідає тому, що дав сервіс
            Assert.Equal(expectedErrorMessage, notFoundResult.Value);
        }
    }
}