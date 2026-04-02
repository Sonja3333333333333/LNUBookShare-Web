using Moq;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using Microsoft.Extensions.Logging; // Обов'язково додаємо цей юзінг

namespace LNUBookShare.UnitTests.BookSearchTests;

public abstract class BookSearchServiceTestBase
{
    protected readonly Mock<IBookRepository> _bookRepoMock;
    protected readonly Mock<ILogger<BookSearchService>> _loggerMock; // Додаємо мок логера
    protected readonly BookSearchService _searchService;

    protected BookSearchServiceTestBase()
    {
        _bookRepoMock = new Mock<IBookRepository>();
        _loggerMock = new Mock<ILogger<BookSearchService>>(); // Ініціалізуємо його

        // Тепер передаємо ОБИДВА об'єкти в конструктор
        _searchService = new BookSearchService(_bookRepoMock.Object, _loggerMock.Object);
    }
}