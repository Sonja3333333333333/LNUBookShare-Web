using System.Collections.Generic;
using System.Threading.Tasks;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Services
{
    public class BookSearchService
    {
        private readonly IBookRepository _repository;

        public BookSearchService(IBookRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<Book>> SearchAsync(string query, string searchBy = "title")
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await _repository.GetAllAsync();
            }

            var results = await _repository.SearchBooksAsync(query.Trim(), searchBy);
            return results;
        }
    }
}
