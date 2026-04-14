namespace LNUBookShare.Application.Interfaces
{
    public interface IChatNotificationService
    {
        Task NotifyNewMessageAsync(int receiverId, int senderId, string content);

        Task NotifyStatusChangedAsync(int userId, bool isOnline);
    }
}