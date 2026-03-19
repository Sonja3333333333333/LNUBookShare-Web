using Moq;
using Xunit;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.UnitTests.BookSearchTests;

public class BookSearchByParametersTests : BookSearchServiceTestBase
{
    [Fact]
    public async Task Search_ByAuthor_PassesCorrectCriteriaToRepo()
    {
        // Arrange
        var query = "Франко";
        var searchBy = "author";

        // Act
        await _searchService.SearchAsync(query, searchBy);

        // Assert
        _bookRepoMock.Verify(r => r.SearchBooksAsync(query, "author", "title", "all"), Times.Once);
    }

    [Fact]
    public async Task Search_ByIsbn_PassesCorrectCriteriaToRepo()
    {
        // Arrange
        var query = "978123";
        var searchBy = "isbn";

        // Act
        await _searchService.SearchAsync(query, searchBy);

        // Assert
        _bookRepoMock.Verify(r => r.SearchBooksAsync(query, "isbn", "title", "all"), Times.Once);
    }

    [Fact]
    public async Task Search_ShouldTrimQuery_BeforePassingToRepo()
    {
        // Arrange
        var dirty = "  Кобзар  ";
        var clean = "Кобзар"; // Тепер тут кирилиця, щоб співпадало з dirty

        // Act
        await _searchService.SearchAsync(dirty);

        // Assert
        // Використовуємо нашу змінну clean тут:
        _bookRepoMock.Verify(r => r.SearchBooksAsync(clean, "title", "title", "all"), Times.Once);
    }
}