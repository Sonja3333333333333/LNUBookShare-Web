using Moq;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;

namespace LNUBookShare.UnitTests.BookSearchTests;

public abstract class BookSearchServiceTestBase
{
    protected readonly Mock<IBookRepository> _bookRepoMock;
    protected readonly BookSearchService _searchService;

    protected BookSearchServiceTestBase()
    {
        _bookRepoMock = new Mock<IBookRepository>();
        _searchService = new BookSearchService(_bookRepoMock.Object);
    }
}