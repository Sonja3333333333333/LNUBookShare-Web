using LNUBookShare.Application.Interfaces;
using LNUBookShare.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LNUBookShare.Web.Services
{
    // Цей клас живе у Web і знає про SignalR, але реалізує інтерфейс із Application
    public class SignalRNotificationSender : IRealTimeNotificationSender
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public SignalRNotificationSender(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendToUserAsync(int userId, string message)
        {
            // Відправляємо пуш конкретному юзеру
            await _hubContext.Clients.Group(userId.ToString()).SendAsync("ReceiveNotification", message);
        }
    }
}