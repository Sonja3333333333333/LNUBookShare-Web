using Moq;
using Xunit;
using LNUBookShare.Domain.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LNUBookShare.UnitTests.BookSearchTests;

public class BookSearchServiceLogicTests : BookSearchServiceTestBase
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public async Task SearchAsync_WhenQueryIsEmpty_ShouldReturnFailure_WithoutCallingRepo(string? emptyQuery)
    {
        // Act
        var result = await _searchService.SearchAsync(emptyQuery!);

        // Assert
        Assert.True(result.IsFailure);
        _bookRepoMock.Verify(r => r.SearchBooksAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WhenNoBooksFound_ShouldReturnSuccessAndEmptyCollection()
    {
        // Arrange
        var query = "Неіснуюча Книга";
        _bookRepoMock
            .Setup(r => r.SearchBooksAsync("title", query, "title", "all"))
            .ReturnsAsync(new List<Book>());

        // Act
        var result = await _searchService.SearchAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Empty(result.Value);
    }

    [Fact]
    public async Task SearchAsync_WhenBooksExist_ShouldReturnSuccessAndBooksCorrectly()
    {
        // Arrange
        var query = "C#";
        var expectedBooks = new List<Book>
        {
            new Book { Title = "C# in Depth" },
            new Book { Title = "C# Via CLR" }
        };

        _bookRepoMock
            .Setup(r => r.SearchBooksAsync("title", query, "title", "all"))
            .ReturnsAsync(expectedBooks);

        // Act
        var result = await _searchService.SearchAsync(query);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count());
        Assert.Contains(result.Value, b => b.Title == "C# in Depth");
    }

    [Fact]
    public async Task SearchAsync_WithFilter_ShouldReturnSuccessAndOnlyFilteredBooks()
    {
        // Arrange
        var query = "Java";
        var status = "available";
        var filteredBooks = new List<Book>
        {
            new Book { Title = "Java Core", Status = "available" }
        };

        _bookRepoMock
            .Setup(r => r.SearchBooksAsync("title", query, "title", status))
            .ReturnsAsync(filteredBooks);

        // Act
        var result = await _searchService.SearchAsync(query, "title", "title", status);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.All(result.Value, b => Assert.Equal("available", b.Status));
    }
}