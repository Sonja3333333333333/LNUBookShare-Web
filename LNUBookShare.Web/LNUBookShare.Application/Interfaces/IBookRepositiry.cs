using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllAsync();// Read (всі)
        Task<Book?> GetByIdAsync(int id);// Read (одна)
        Task AddAsync(Book book);// Create
        Task UpdateAsync(Book book);// Update
        Task DeleteAsync(Book book);// Delete

        Task ClearAllAsync();
    }
}