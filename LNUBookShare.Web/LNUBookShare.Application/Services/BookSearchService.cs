using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging; // Додай цей юзінг

namespace LNUBookShare.Application.Services
{
    public class BookSearchService : IBookSearchService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<BookSearchService> _logger; // Додай це поле

        // Онови конструктор, щоб він приймав ДВА параметри
        public BookSearchService(IBookRepository bookRepository, ILogger<BookSearchService> logger)
        {
            _bookRepository = bookRepository;
            _logger = logger;
        }

        public async Task<IEnumerable<Book>> SearchAsync(string query, string searchBy = "title", string sortBy = "title", string statusFilter = "all")
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _logger.LogInformation("Пошук з порожнім запитом."); // Тепер можеш логувати!
                return Enumerable.Empty<Book>();
            }

            var trimmedQuery = query.Trim();

            return await _bookRepository.SearchBooksAsync(searchBy, trimmedQuery, sortBy, statusFilter);
        }

        public async Task<IEnumerable<Book>> GetRecommendationsAsync(int facultyId, int currentUserId, string sortBy = "title", string statusFilter = "all")
        {
            return await _bookRepository.GetRecommendationsAsync(facultyId, currentUserId, sortBy, statusFilter);
        }
    }
}