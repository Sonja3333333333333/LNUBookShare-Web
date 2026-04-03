using Moq;
using Xunit;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.UnitTests.BookSearchTests;

public class BookSearchSortingTests : BookSearchServiceTestBase
{
    [Fact]
    public async Task Search_SortByYear_PassesYearParamToRepo()
    {
        // Arrange
        var sortBy = "year";

        // Act
        await _searchService.SearchAsync("алгебра", "title", sortBy);

        // Assert
        _bookRepoMock.Verify(r => r.SearchBooksAsync("title", "алгебра", "year", "all"), Times.Once);
    }

    [Fact]
    public async Task Search_SortByAuthor_PassesAuthorParamToRepo()
    {
        // Arrange
        var sortBy = "author";

        // Act
        await _searchService.SearchAsync("алгебра", "title", sortBy);

        // Assert
        _bookRepoMock.Verify(r => r.SearchBooksAsync("title", "алгебра", "author", "all"), Times.Once);
    }
}