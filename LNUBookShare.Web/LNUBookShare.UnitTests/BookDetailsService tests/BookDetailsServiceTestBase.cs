using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace LNUBookShare.UnitTests.BookDetailsService_tests
{
    public class BookDetailsServiceTestBase
    {
        protected readonly Mock<IBookRepository> _bookRepoMock;
        protected readonly BookDetailsService _bookDetailsService;

        protected BookDetailsServiceTestBase()
        {
            _bookRepoMock = new Mock<IBookRepository>();
            _bookDetailsService = new BookDetailsService(_bookRepoMock.Object, new Mock<ILogger<BookDetailsService>>().Object);
        } 

        
    }
}