using System.Collections.Concurrent;
using Microsoft.AspNetCore.SignalR;

namespace LNUBookShare.Web.Hubs
{
    public class ChatHub : Hub
    {
        // Ключ - UserId, Значення - кількість відкритих вкладок (щоб не "офлайнити" при закритті лише однієї)
        private static readonly ConcurrentDictionary<int, int> OnlineUsers = new ();

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userIdString = Context.UserIdentifier; // Identity автоматично підставляє сюди ID
            if (int.TryParse(userIdString, out int userId))
            {
                if (OnlineUsers.TryRemove(userId, out _))
                {
                    // Кажемо всім, що юзер вийшов
                    await Clients.All.SendAsync("UserStatusChanged", userId, false);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinPrivateChat(string userIdStr)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, userIdStr);

            if (int.TryParse(userIdStr, out int userId))
            {
                OnlineUsers.TryAdd(userId, 1);
                await Clients.All.SendAsync("UserStatusChanged", userId, true);
            }
        }

        // Метод, щоб запитати, чи юзер онлайн прямо зараз при завантаженні сторінки
        public bool IsUserOnline(int userId)
        {
            return OnlineUsers.ContainsKey(userId);
        }
    }
}