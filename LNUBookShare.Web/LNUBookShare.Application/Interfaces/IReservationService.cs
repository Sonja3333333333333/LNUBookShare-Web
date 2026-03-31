using LNUBookShare.Application.Common;

namespace LNUBookShare.Application.Interfaces
{
    public interface IReservationService
    {
        Task<Result> ReserveBookAsync(int bookId, int userId);
        Task<Result> JoinQueueAsync(int bookId, int userId);
        Task<int> GetQueuePositionAsync(int bookId, int userId);
        Task<bool> IsUserInQueueAsync(int bookId, int userId);
    }
}