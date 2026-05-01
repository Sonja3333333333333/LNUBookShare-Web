using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IRentalTransactionRepository
    {
        Task AddAsync(RentalTransaction transaction);
        Task<RentalTransaction?> GetActiveByBookIdAsync(int bookId);
        Task UpdateAsync(RentalTransaction transaction);

        Task<IEnumerable<RentalTransaction>> GetAllWithDetailsAsync(string? searchBy, string? searchQuery, string? sortBy, string? statusFilter, string? termFilter);
    }
}