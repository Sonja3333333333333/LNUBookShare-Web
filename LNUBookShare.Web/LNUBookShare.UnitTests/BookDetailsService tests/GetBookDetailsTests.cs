using LNUBookShare.Domain.Entities;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace LNUBookShare.UnitTests.BookDetailsService_tests
{
    public class GetBookDetailsTests : BookDetailsServiceTestBase
    {
        [Fact]
        public async Task GetBookDetailsAsync_WhenBookExists_ShouldReturnSuccessWithBook()
        {
            // Arrange
            int bookId = 1;
            var expectedBook = new Book
            {
                BookId = bookId,
                Title = "Кобзар",
                Owner = new User { FirstName = "Тарас" },
                Cover = new Image { ImagePath = "cover.jpg" },
                BookReviews = new List<BookReview>()
            };

            // Налаштовуємо репозиторій
            _bookRepoMock
                .Setup(r => r.GetByIdMoreDetailsAsync(bookId))
                .ReturnsAsync(expectedBook);

            // Act
            var result = await _bookDetailsService.GetBookDetailsAsync(bookId);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Value);
            Assert.Equal(expectedBook.Title, result.Value.Title);

            // Перевіряємо всі  Include 
            Assert.NotNull(result.Value.Owner);
            Assert.Equal("Тарас", result.Value.Owner.FirstName);


            _bookRepoMock.Verify(r => r.GetByIdMoreDetailsAsync(bookId), Times.Once);
        }

        [Fact]
        public async Task GetBookDetailsAsync_WhenBookDoesNotExist_ShouldReturnFailure()
        {
            // Arrange
            int nonExistentId = 999;

            _bookRepoMock
                .Setup(r => r.GetByIdMoreDetailsAsync(nonExistentId))
                .ReturnsAsync((Book?)null);

            // Act
            var result = await _bookDetailsService.GetBookDetailsAsync(nonExistentId);

            // Assert
            Assert.True(result.IsFailure);
            Assert.Equal("Книга не знайдена", result.Error);            
        }
    }
}
