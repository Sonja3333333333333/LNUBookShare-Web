using LNUBookShare.Application.Interfaces;
using LNUBookShare.Web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace LNUBookShare.Web.BackgroundServices;

public class NotificationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationWorker> _logger;

    public NotificationWorker(IServiceScopeFactory scopeFactory, ILogger<NotificationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogError("!!! [WORKER CHECK] Поточний час: {Time} !!!", DateTime.Now.ToString("HH:mm:ss"));

            using (var scope = _scopeFactory.CreateScope())
            {
                var rentalRepo = scope.ServiceProvider.GetRequiredService<IRentalTransactionRepository>();
                var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var bookRepo = scope.ServiceProvider.GetRequiredService<IBookRepository>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

                // --- ПОДІЯ №1: Нагадування про дедлайн (1 раз на добу) ---
                var tomorrow = DateTime.UtcNow.AddDays(-330).Date;
                var expiring = await rentalRepo.GetExpiringRentalsAsync(tomorrow, stoppingToken);

                foreach (var r in expiring)
                {
                    string checkMsgPart = $"повернути книгу '{r.Book.Title}'";
                    bool alreadySentToday = await notificationRepo.ExistsTodayAsync(r.BorrowerId, r.BookId, checkMsgPart);

                    if (!alreadySentToday)
                    {
                        await notificationService.CreateNotificationAsync(
                            r.BorrowerId,
                            $"Нагадування: завтра необхідно {checkMsgPart}.",
                            r.BookId);
                    }
                }

                // --- ПОДІЯ №2: "спам"  ---
                var staleThreshold = DateTime.UtcNow.AddSeconds(-1);
                var usersToNotify = await notificationRepo.GetUserIdsWithPendingNotificationsAsync(staleThreshold);

                foreach (var userId in usersToNotify)
                {
                    await hubContext.Clients.Group(userId.ToString()).SendAsync(
                        "ReceiveNotification",
                        "У вас є непрочитані сповіщення. Будь ласка, перегляньте їх у вкладці 'Сповіщення'.");
                }

                // --- ПОДІЯ №3: (Топ-1 користувач місяця) ---
                var monthAgo = DateTime.UtcNow.AddDays(-30);
                var topList = await bookRepo.GetTopActiveUsersWithRecentBooksAsync(monthAgo, 1);
                var leader = topList.FirstOrDefault();

                if (leader != null)
                {
                    string winMsg = "Вітаємо! Ви стали користувачем №1 цього місяця за кількістю доданих книг! 🏆";

                    bool alreadyWonToday = await notificationRepo.ExistsTodayAsync(leader.UserId, null, "користувачем №1");

                    if (!alreadyWonToday)
                    {
                        await notificationService.CreateNotificationAsync(leader.UserId, winMsg, null);
                        _logger.LogInformation("Юзер {UserId} отримав титул ТОП-1 місяця", leader.UserId);
                    }
                }
            }

            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}