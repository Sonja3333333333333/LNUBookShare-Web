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
        private readonly Mock<ILogger<BookDetailsService>> _loggerMock;
        protected readonly BookDetailsService _bookDetailsService;

        protected BookDetailsServiceTestBase()
        {
            _bookRepoMock = new Mock<IBookRepository>();
            _loggerMock = new Mock<ILogger<BookDetailsService>>();
            _bookDetailsService = new BookDetailsService(_bookRepoMock.Object, _loggerMock.Object);
        } 

        
    }
}