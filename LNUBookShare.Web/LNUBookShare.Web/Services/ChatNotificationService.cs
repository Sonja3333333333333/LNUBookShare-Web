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
            var time = DateTime.UtcNow.ToString("HH:mm");

            // 1. Шлемо отримувачу (щоб у нього з'явився бабл)
            await _hubContext.Clients.Group(receiverId.ToString())
                .SendAsync("ReceiveMessage", senderId, content, time);

            // 2. Шлемо відправнику (тобі), щоб твій JS теж намалював бабл
            // Це важливо, бо ми не рефрешимо сторінку через AJAX!
            await _hubContext.Clients.Group(senderId.ToString())
                .SendAsync("ReceiveMessage", senderId, content, time);
        }

        public async Task NotifyStatusChangedAsync(int userId, bool isOnline)
        {
            await _hubContext.Clients.All.SendAsync("UserStatusChanged", userId, isOnline);
        }
    }
}