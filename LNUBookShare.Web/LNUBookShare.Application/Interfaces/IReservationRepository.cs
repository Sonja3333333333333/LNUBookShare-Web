using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IReservationRepository
    {
        Task AddAsync(ReservationQueue entry);
        Task<bool> ExistsAsync(int bookId, int userId);
        Task<int> GetPositionAsync(int bookId, int userId);
    }
}