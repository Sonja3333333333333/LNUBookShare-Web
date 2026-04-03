using Moq;
using Xunit;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.UnitTests.BookSearchTests;

public class BookSearchSortingTests : BookSearchServiceTestBase
{
    [Fact]
    public async Task Search_SortByYear_PassesYearParamToRepo()
    {
        // Act
        await _searchService.SearchAsync("алгебра", "title", "year");

        // Assert: Перевіряємо правильну послідовність у Verify
        _bookRepoMock.Verify(r => r.SearchBooksAsync("title", "алгебра", "year", "all"), Times.Once);
    }

}