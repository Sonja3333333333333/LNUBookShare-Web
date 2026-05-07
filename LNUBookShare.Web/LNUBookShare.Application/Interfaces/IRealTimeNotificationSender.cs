using System.Threading.Tasks;

namespace LNUBookShare.Application.Interfaces
{
    public interface IRealTimeNotificationSender
    {
        Task SendToUserAsync(int userId, string message);
    }
}