using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Core;

namespace LNUBookShare.Application.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepo;
        private readonly IBookRepository _bookRepo;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ReservationService> _logger;
        private readonly ReservationSettings _settings;

        public ReservationService(
            IReservationRepository reservationRepo,
            IBookRepository bookRepo,
            INotificationService notificationService,
            ILogger<ReservationService> logger,
            IOptions<ReservationSettings> options)
        {
            _reservationRepo = reservationRepo;
            _bookRepo = bookRepo;
            _notificationService = notificationService;
            _logger = logger;
            _settings = options.Value;
        }

        public async Task<Result> ReserveBookAsync(int bookId, int userId)
        {
            _logger.LogInformation("Спроба бронювання книги {BookId} користувачем {UserId}", bookId, userId);

            var book = await _bookRepo.GetByIdAsync(bookId);
            if (book == null)
            {
                _logger.LogWarning("Помилка бронювання: Книгу {BookId} не знайдено.", bookId);
                return Result.Failure("Книгу не знайдено.");
            }

            if (book.Status != "available")
            {
                _logger.LogWarning("Помилка бронювання: Книга {BookId} має статус '{Status}' і недоступна.", bookId, book.Status);
                return Result.Failure("Книга вже недоступна для бронювання.");
            }

            if (book.OwnerId == userId)
            {
                _logger.LogWarning("Помилка бронювання: Користувач {UserId} намагається забронювати власну книгу {BookId}.", userId, bookId);
                return Result.Failure("Ви не можете бронювати власну книгу.");
            }

            book.Status = "reserved";
            await _bookRepo.UpdateAsync(book);

            var entry = new ReservationQueue
            {
                BookId = bookId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            };

            await _reservationRepo.AddAsync(entry);
            _logger.LogInformation("Користувач {UserId} успішно забронював книгу {BookId}", userId, bookId);

            await _notificationService.CreateNotificationAsync(
                book.OwnerId,
                $"Вашу книгу «{book.Title}» щойно забронювали. Будь ласка, перегляньте деталі.",
                bookId);

            return Result.Success();
        }

        public async Task<Result> JoinQueueAsync(int bookId, int userId)
        {
            _logger.LogInformation("Спроба користувача {UserId} приєднатися до черги на книгу {BookId}", userId, bookId);
            var book = await _bookRepo.GetByIdAsync(bookId);
            if (book == null)
            {
                _logger.LogWarning("Помилка додавання в чергу: Книгу {BookId} не знайдено.", bookId);
                return Result.Failure("Книгу не знайдено.");
            }

            if (book.Status == "available")
            {
                _logger.LogInformation("Користувач {UserId} спробував стати в чергу на вільну книгу {BookId}.", userId, bookId);
                return Result.Failure("Книга вільна. Використовуйте бронювання.");
            }

            if (await _reservationRepo.ExistsAsync(bookId, userId))
            {
                _logger.LogWarning("Користувач {UserId} вже перебуває у черзі на книгу {BookId}.", userId, bookId);
                return Result.Failure("Ви вже у черзі.");
            }

            var currentQueue = await _reservationRepo.GetQueueUsersAsync(bookId);
            if (currentQueue.Count >= _settings.MaxQueueSize)
            {
                _logger.LogWarning("Користувач {UserId} не зміг стати в чергу на книгу {BookId}. Черга переповнена (Макс: {MaxSize}).", userId, bookId, _settings.MaxQueueSize);
                return Result.Failure($"Черга на цю книгу переповнена. Максимум {_settings.MaxQueueSize} осіб.");
            }

            var entry = new ReservationQueue
            {
                BookId = bookId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            };

            await _reservationRepo.AddAsync(entry);

            _logger.LogInformation("Користувача {UserId} успішно додано до черги на книгу {BookId}", userId, bookId);

            await _notificationService.CreateNotificationAsync(
                book.OwnerId,
                $"Хтось щойно став у чергу на вашу книгу «{book.Title}».",
                bookId);

            return Result.Success();
        }

        public async Task<Result<int>> GetQueuePositionAsync(int bookId, int userId)
        {
            return await _reservationRepo.GetPositionAsync(bookId, userId);
        }

        public async Task<Result<bool>> IsUserInQueueAsync(int bookId, int userId)
        {
            return await _reservationRepo.ExistsAsync(bookId, userId);
        }

        public async Task<Result<List<User>>> GetQueueUsersAsync(int bookId)
        {
            _logger.LogInformation("Отримання списку користувачів у черзі для книги з ID {BookId}", bookId);

            var usersInQueue = await _reservationRepo.GetQueueUsersAsync(bookId);

            if (!usersInQueue.Any())
            {
                _logger.LogInformation("Черга для книги з ID {BookId} порожня", bookId);
            }
            else
            {
                _logger.LogInformation("Знайдено {Count} користувачів у черзі для книги з ID {BookId}", usersInQueue.Count, bookId);
            }

            return usersInQueue;
        }

        public async Task<Result> LeaveQueueAsync(int bookId, int userId)
        {
            _logger.LogInformation("Спроба користувача {UserId} покинути чергу на книгу {BookId}", userId, bookId);

            var queueResult = await GetQueueUsersAsync(bookId);
            if (queueResult.IsFailure)
            {
                _logger.LogError("Помилка при перевірці черги на книгу {BookId}", bookId);
                return Result.Failure("Помилка при перевірці черги.");
            }

            var usersInQueue = queueResult.Value.ToList();
            var queueItem = await _reservationRepo.GetByUserAndBookAsync(userId, bookId);

            if (queueItem == null)
            {
                _logger.LogWarning("Користувача {UserId} не знайдено в черзі на книгу {BookId}", userId, bookId);
                return Result.Failure("Користувача не знайдено в черзі.");
            }

            bool isFirstInQueue = usersInQueue.FirstOrDefault()?.Id == userId;

            await _reservationRepo.DeleteAsync(queueItem);

            _logger.LogInformation("Користувач {UserId} успішно покинув чергу на книгу {BookId}", userId, bookId);

            if (usersInQueue.Count == 1)
            {
                await MakeBookAvailableAsync(bookId);
            }
            else if (isFirstInQueue && usersInQueue.Count > 1)
            {
                await NotifyNextUserInQueueAsync(bookId, usersInQueue[0], usersInQueue[1]);
            }

            return Result.Success();
        }

        private async Task MakeBookAvailableAsync(int bookId)
        {
            _logger.LogInformation("Оскільки черга спорожніла, спроба змінити статус книги {BookId} на 'available'", bookId);
            var book = await _bookRepo.GetByIdAsync(bookId);
            if (book != null)
            {
                book.Status = "available";
                await _bookRepo.UpdateAsync(book);
                _logger.LogInformation("Статус книги {BookId} успішно змінено на 'available'", bookId);
            }
            else
            {
                _logger.LogWarning("Не вдалося змінити статус: книгу {BookId} не знайдено", bookId);
            }
        }

        private async Task NotifyNextUserInQueueAsync(int bookId, User leavingUser, User newFirstUser)
        {
            _logger.LogInformation("Користувач {UserId} став першим у черзі на книгу {BookId}. Надсилаємо сповіщення.", newFirstUser.Id, bookId);
            var book = await _bookRepo.GetByIdAsync(bookId);
            if (book != null)
            {
                string message = $"Користувач {leavingUser.FirstName} {leavingUser.LastName} вийшов із черги на книгу '{book.Title}', і тепер вона автоматично заброньована за вами! Зв'яжіться з власником.";

                await _notificationService.CreateNotificationAsync(newFirstUser.Id, message, bookId);
            }
            else
            {
                _logger.LogWarning("Не вдалося надіслати сповіщення наступному в черзі: книгу {BookId} не знайдено", bookId);
            }
        }
    }
}