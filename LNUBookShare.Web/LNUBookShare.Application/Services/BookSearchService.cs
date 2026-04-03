using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class BookSearchService : IBookSearchService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<BookSearchService> _logger;

        public BookSearchService(IBookRepository bookRepository, ILogger<BookSearchService> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public async Task<Result<IEnumerable<Book>>> SearchAsync(string query, string searchBy = "title", string sortBy = "title", string statusFilter = "all")
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogWarning("Спроба пошуку з порожнім запитом.");
                return Result<IEnumerable<Book>>.Failure("Пошуковий запит не може бути порожнім.");
            }

            var trimmedQuery = query.Trim();
            var books = await _bookRepository.SearchBooksAsync(searchBy, trimmedQuery, sortBy, statusFilter);

            return Result<IEnumerable<Book>>.Success(books);
        }

        public async Task<Result<IEnumerable<Book>>> GetRecommendationsAsync(int facultyId, int currentUserId, string sortBy = "title", string statusFilter = "all")
        {
            var books = await _bookRepository.GetRecommendationsAsync(facultyId, currentUserId, sortBy, statusFilter);

            return Result<IEnumerable<Book>>.Success(books);
        }
    }
}