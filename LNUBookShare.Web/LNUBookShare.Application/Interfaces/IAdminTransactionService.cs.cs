using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IAdminTransactionService
    {
        Task<Result<IEnumerable<RentalTransaction>>> GetTransactionsAsync(string? searchBy, string? searchQuery, string? sortBy, string? statusFilter, string? termFilter);
    }
}
