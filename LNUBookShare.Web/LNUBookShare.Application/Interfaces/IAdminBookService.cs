using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Models;

namespace LNUBookShare.Application.Interfaces
{
    public interface IAdminBookService
    {
        Task<Result<IEnumerable<AdminBookDto>>> GetAllBooksAsync();
        Task<Result> DeleteBookAsync(int bookId);
    }

}