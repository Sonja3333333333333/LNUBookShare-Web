using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Infrastructure;
using LNUBookShare.Infrastructure.Data;
using LNUBookShare.Web.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // Додано для логера

namespace LNUBookShare.Web.BackgroundServices;

public class NotificationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationWorker> _logger; // Поле для логера

    public NotificationWorker(IServiceScopeFactory scopeFactory, ILogger<NotificationWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // ЧЕРВОНЕ ПОВІДОМЛЕННЯ: Виводиться щохвилини перед початком перевірки
            _logger.LogError("!!! [WORKER CHECK] Поточний час: {Time} !!!", DateTime.Now.ToString("HH:mm:ss"));

            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
                var hubContext = scope.ServiceProvider.GetRequiredService<IHubContext<NotificationHub>>();

                var tomorrow = DateTime.UtcNow.AddDays(1).Date;

                // Шукаємо тих, хто має повернути книгу завтра
                var expiringRentals = await dbContext.RentalTransactions
                    .Include(r => r.Book)
                    .Where(r => r.ExpectedReturnDate.Date == tomorrow && r.Status == TransactionStatuses.Active)
                    .ToListAsync(stoppingToken);

                foreach (var rental in expiringRentals)
                {
                    string msg = $"Нагадування: завтра необхідно повернути книгу '{rental.Book.Title}'.";

                    // 1. Зберігаємо в базу
                    await notificationService.CreateNotificationAsync(rental.BorrowerId, msg, rental.BookId);

                    // 2. Шлемо пуш через SignalR
                    await hubContext.Clients.Group(rental.BorrowerId.ToString()).SendAsync("ReceiveNotification", msg);
                }
            }

            // Затримка на 1 хвилину
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }
}