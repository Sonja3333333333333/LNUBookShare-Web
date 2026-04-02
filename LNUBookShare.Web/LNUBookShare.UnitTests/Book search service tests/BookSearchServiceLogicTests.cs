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
    public async Task SearchAsync_WhenQueryIsEmpty_ShouldReturnEmptyList_WithoutCallingRepo(string? emptyQuery)
    {
        // Act
        var result = await _searchService.SearchAsync(emptyQuery!);

        // Дістаємо сам список із результату
        var books = result.Value;

        // Assert
        Assert.True(result.IsSuccess); // Перевіряємо, що статус успішний
        Assert.NotNull(books);
        Assert.Empty(books); // Тепер Assert.Empty працює зі списком!

        // Перевіряємо, що репозиторій навіть не смикали (економія ресурсів!)
        _bookRepoMock.Verify(r => r.SearchBooksAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task SearchAsync_WhenNoBooksFound_ShouldReturnEmptyCollection()
    {
        // Arrange
        var query = "Неіснуюча Книга";
        _bookRepoMock
            .Setup(r => r.SearchBooksAsync(query, "title", "title", "all"))
            .ReturnsAsync(new List<Book>());

        // Act
        var result = await _searchService.SearchAsync(query);
        var books = result.Value; // Дістаємо список

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(books);
        Assert.Empty(books);
    }

    [Fact]
    public async Task SearchAsync_WhenBooksExist_ShouldReturnThemCorrectly()
    {
        // Arrange
        var query = "C#";
        var expectedBooks = new List<Book>
        {
            new Book { Title = "C# in Depth" },
            new Book { Title = "C# Via CLR" }
        };

        _bookRepoMock
            .Setup(r => r.SearchBooksAsync(query, "title", "title", "all"))
            .ReturnsAsync(expectedBooks);

        // Act
        var result = await _searchService.SearchAsync(query);
        var books = result.Value; // Дістаємо список

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(2, books.Count());
        Assert.Contains(books, b => b.Title == "C# in Depth");
    }

    [Fact]
    public async Task SearchAsync_WithFilter_ShouldReturnOnlyFilteredBooks()
    {
        // Arrange
        var query = "Java";
        var status = "available";
        var filteredBooks = new List<Book> { new Book { Title = "Java Core", Status = "available" } };

        _bookRepoMock
            .Setup(r => r.SearchBooksAsync(query, "title", "title", status))
            .ReturnsAsync(filteredBooks);

        // Act
        var result = await _searchService.SearchAsync(query, "title", "title", status);
        var books = result.Value; // Дістаємо список

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Single(books); // Має бути рівно 1 книга
        Assert.All(books, b => Assert.Equal("available", b.Status));
    }
}