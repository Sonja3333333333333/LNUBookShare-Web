using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using Microsoft.Extensions.Logging; // 1. Додай цей юзінг
using Moq;

namespace LNUBookShare.UnitTests.BookDetailsService_tests
{
    public class BookDetailsServiceTestBase
    {
        protected readonly Mock<IBookRepository> _bookRepoMock;
        protected readonly Mock<ILogger<BookDetailsService>> _loggerMock; // 2. Створи мок для логера
        protected readonly BookDetailsService _bookDetailsService;

        protected BookDetailsServiceTestBase()
        {
            _bookRepoMock = new Mock<IBookRepository>();
            _loggerMock = new Mock<ILogger<BookDetailsService>>(); // 3. Ініціалізуй його

            // 4. Тепер передай два об'єкти в конструктор
            _bookDetailsService = new BookDetailsService(_bookRepoMock.Object, _loggerMock.Object);
        }
    }
}