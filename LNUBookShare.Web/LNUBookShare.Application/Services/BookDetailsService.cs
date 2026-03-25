using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class BookDetailsService : IBookDetailsService
    {
        private readonly IBookRepository _repository;
        private readonly ILogger<BookDetailsService> _logger;

        public BookDetailsService(IBookRepository repository, ILogger<BookDetailsService> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<Result<Book>> GetBookDetailsAsync(int book_id)
        {
            var book = await _repository.GetByIdMoreDetailsAsync(book_id);

            if (book is null)
            {
                _logger.LogWarning("Спроба перегляду деталей неіснуючої книги з ID: {BookId}", book_id);
                return Result<Book>.Failure("Книга не знайдена");
            }

            _logger.LogInformation("Успішно отримано деталі для книги з ID: {BookId}", book_id);
            return Result<Book>.Success(book);
        }
    }
}
