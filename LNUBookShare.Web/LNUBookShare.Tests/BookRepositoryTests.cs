using LNUBookShare.Application.Models;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Infrastructure;
using LNUBookShare.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Tests
{
    public class BookRepositoryTests
    {
        private AppDbContext CreateInMemoryContext()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            var context = new AppDbContext(options);

            context.Books.AddRange(
                new Book { BookId = 1, Title = "ASP.NET Core", Author = "A", Year = 2020, Status = "Available", CategoryId = 1 },
                new Book { BookId = 2, Title = "Clean Code", Author = "B", Year = 2022, Status = "Available", CategoryId = 1 },
                new Book { BookId = 3, Title = "Zelda Guide", Author = "C", Year = 2018, Status = "Borrowed", CategoryId = 2 }
            );
            context.SaveChanges();
            return context;
        }

        [Fact]
        public async Task GetBooks_SortByYearDescending_ReturnsCorrectOrder()
        {
            var context = CreateInMemoryContext();
            var repo = new BookRepository(context);

            var result = (await repo.GetFilteredAsync(new BookFilterParams { SortBy = "year" })).ToList();

            Assert.Equal(2022, result[0].Year);
            Assert.Equal(2020, result[1].Year);
            Assert.Equal(2018, result[2].Year);
        }

        [Fact]
        public async Task GetBooks_FilterByStatusAvailable_ReturnsOnlyAvailableBooks()
        {
            var context = CreateInMemoryContext();
            var repo = new BookRepository(context);

            var result = await repo.GetFilteredAsync(new BookFilterParams { Status = "Available" });

            Assert.All(result, b => Assert.Equal("Available", b.Status));
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetBooks_InvalidSortParameter_UsesDefaultSorting()
        {
            var context = CreateInMemoryContext();
            var repo = new BookRepository(context);

            var result = (await repo.GetFilteredAsync(new BookFilterParams { SortBy = "invalid_xyz" })).ToList();

            Assert.Equal("ASP.NET Core", result[0].Title);
            Assert.Equal("Clean Code", result[1].Title);
            Assert.Equal("Zelda Guide", result[2].Title);
        }
    }
}