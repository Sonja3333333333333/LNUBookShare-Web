using Moq;
using Xunit;
using LNUBookShare.Application.Services;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;


namespace LNUBookShare.UnitTests
{
    public class BookSearchServiceTests
    {
        private readonly Mock<IBookRepository> _bookRepoMock;
        private readonly BookSearchService _searchService;

        public BookSearchServiceTests()
        {            
            _bookRepoMock = new Mock<IBookRepository>();            
            _searchService = new BookSearchService(_bookRepoMock.Object);
        }

        [Fact]
        public async Task Search_KeywordExists_ReturnsMatchingBooks()
        {
            // Arrange 
            var query = "Алгебра";
            var expectedBooks = new List<Book>
            {
                new Book { BookId = 1, Title = "Вища Алгебра" }
            };
            
            _bookRepoMock.Setup(r => r.SearchBooksAsync(query, "title"))
                         .ReturnsAsync(expectedBooks);

            // Act 
            var result = await _searchService.SearchAsync(query);

            // Assert 
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Вища Алгебра", result.First().Title);
        }

        [Fact]
        public async Task Search_KeywordNotFound_ReturnsEmpty()
        {
            // Arrange
            var query = "Неіснуюча Книга";
            
            _bookRepoMock.Setup(r => r.SearchBooksAsync(query, "title"))
                         .ReturnsAsync(new List<Book>());

            // Act
            var result = await _searchService.SearchAsync(query);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task Search_EmptyQuery_ReturnsAllBooks()
        {
            // Arrange 
            var query = "";
            var allBooks = new List<Book>
            {
                new Book { BookId = 1, Title = "Алгебра" },
                new Book { BookId = 2, Title = "Архітектура" }
            };

            
            _bookRepoMock.Setup(r => r.GetAllAsync())
                         .ReturnsAsync(allBooks);

            // Act
            var result = await _searchService.SearchAsync(query);

            // Assert            
            Assert.NotNull(result);
            Assert.Equal(allBooks.Count, result.Count());
        }
    }
}
