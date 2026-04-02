using Moq;
using Xunit;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.UnitTests.BookSearchTests;

public class BookSearchServiceLogicTests : BookSearchServiceTestBase
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SearchAsync_WhenQueryIsEmpty_ShouldReturnFailure_WithoutCallingRepo(string? emptyQuery)
    {
        var result = await _searchService.SearchAsync(emptyQuery!);

        Assert.NotNull(result);
        Assert.True(result.IsFailure);

        _bookRepoMock.Verify(r => r.SearchBooksAsync(
            It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WhenNoBooksFound_ShouldReturnSuccessWithEmptyCollection()
    {
        var query = "Неіснуюча Книга";
        _bookRepoMock
            .Setup(r => r.SearchBooksAsync("title", query, "title", "all"))
            .ReturnsAsync(new List<Book>());

        var result = await _searchService.SearchAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task SearchAsync_WhenBooksExist_ShouldReturnThemCorrectly()
    {
        var query = "C#";
        var expectedBooks = new List<Book>
        {
            new Book { Title = "C# in Depth" },
            new Book { Title = "C# Via CLR" }
        };

        _bookRepoMock
            .Setup(r => r.SearchBooksAsync("title", query, "title", "all"))
            .ReturnsAsync(expectedBooks);

        var result = await _searchService.SearchAsync(query);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
        Assert.Contains(result.Value, b => b.Title == "C# in Depth");
    }

    [Fact]
    public async Task SearchAsync_WithFilter_ShouldReturnOnlyFilteredBooks()
    {
        var query = "Java";
        var status = "available";
        var filteredBooks = new List<Book>
        {
            new Book { Title = "Java Core", Status = "available" }
        };

        _bookRepoMock
            .Setup(r => r.SearchBooksAsync("title", query, "title", status))
            .ReturnsAsync(filteredBooks);

        var result = await _searchService.SearchAsync(query, "title", "title", status);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.All(result.Value, b => Assert.Equal("available", b.Status));
    }
}