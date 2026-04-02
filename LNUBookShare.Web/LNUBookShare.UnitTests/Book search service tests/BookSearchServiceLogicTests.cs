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
    public async Task SearchAsync_WhenQueryIsEmpty_ShouldReturnEmptyList_WithoutCallingRepo(string? emptyQuery)
    {
        // Act
        // Навіть якщо query криве, сервіс має зреагувати
        var result = await _searchService.SearchAsync(emptyQuery!);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);

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

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
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

        // ФІКС: Порядок аргументів (searchBy: "title", query: query)
        _bookRepoMock
            .Setup(r => r.SearchBooksAsync("title", query, "title", "all"))
            .ReturnsAsync(expectedBooks);

        // Act
        var result = await _searchService.SearchAsync(query);

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task SearchAsync_WithFilter_ShouldReturnOnlyFilteredBooks()
    {
        // Arrange
        var query = "Java";
        var status = "available";
        var filteredBooks = new List<Book> { new Book { Title = "Java Core", Status = "available" } };

        // ФІКС: Узгоджуємо аргументи з репозиторієм
        _bookRepoMock
            .Setup(r => r.SearchBooksAsync("title", query, "title", status))
            .ReturnsAsync(filteredBooks);

        // Act
        var result = await _searchService.SearchAsync(query, "title", "title", status);

        // Assert
        Assert.Single(result);
        Assert.All(result, b => Assert.Equal("available", b.Status));
    }
}