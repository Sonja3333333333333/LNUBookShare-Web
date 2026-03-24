using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Services
{
    public class BookDetailsService : IBookDetailsService
    {
        private readonly IBookRepository _repository;
        public BookDetailsService(IBookRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<Book>> GetBookDetailsAsync(int book_id)
        {
            var book = await _repository.GetByIdMoreDetailsAsync(book_id);

            if (book is null)
            {
                return Result<Book>.Failure("Книга не знайдена");
            }

            return Result<Book>.Success(book);
        }
    }
}
