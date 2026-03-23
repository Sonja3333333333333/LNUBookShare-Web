using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IBookDetailsService
    {
        Task<Result<Book>> GetBookDetailsAsync(int book_id);
    }
}
