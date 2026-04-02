using LNUBookShare.Application.Common;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IReservationService
    {
        Task<Result> ReserveBookAsync(int bookId, int userId);
        Task<Result> JoinQueueAsync(int bookId, int userId);
        Task<Result<int>> GetQueuePositionAsync(int bookId, int userId);
        Task<Result<bool>> IsUserInQueueAsync(int bookId, int userId);
        Task<Result<List<User>>> GetQueueUsersAsync(int bookId);
    }
}