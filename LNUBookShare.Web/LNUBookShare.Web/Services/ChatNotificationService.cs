using LNUBookShare.Application.Interfaces;
using LNUBookShare.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LNUBookShare.Web.Services
{
    public class ChatNotificationService : IChatNotificationService
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatNotificationService(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyNewMessageAsync(int receiverId, int senderId, string content)
        {
            await _hubContext.Clients.Group(receiverId.ToString())
                .SendAsync("ReceiveMessage", senderId, content, DateTime.UtcNow.ToString("HH:mm"));
        }

        public async Task NotifyStatusChangedAsync(int userId, bool isOnline)
        {
            await _hubContext.Clients.All.SendAsync("UserStatusChanged", userId, isOnline);
        }
    }
}