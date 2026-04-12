using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class ReservationService : IReservationService
    {
        private readonly IReservationRepository _reservationRepo;
        private readonly IBookRepository _bookRepo;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IReservationRepository reservationRepo,
            IBookRepository bookRepo,
            INotificationService notificationService,
            ILogger<ReservationService> logger)
        {
            _reservationRepo = reservationRepo;
            _bookRepo = bookRepo;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Result> ReserveBookAsync(int bookId, int userId)
        {
            var book = await _bookRepo.GetByIdAsync(bookId);
            if (book == null)
            {
                return Result.Failure("Книгу не знайдено.");
            }

            if (book.Status != "available")
            {
                return Result.Failure("Книга вже недоступна для бронювання.");
            }

            if (book.OwnerId == userId)
            {
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
            _logger.LogInformation("User {UserId} reserved book {BookId}", userId, bookId);

            await _notificationService.CreateNotificationAsync(
                book.OwnerId,
                $"Вашу книгу «{book.Title}» щойно забронювали. Будь ласка, перегляньте деталі.",
                bookId);

            return Result.Success();
        }

        public async Task<Result> JoinQueueAsync(int bookId, int userId)
        {
            var book = await _bookRepo.GetByIdAsync(bookId);
            if (book == null)
            {
                return Result.Failure("Книгу не знайдено.");
            }

            if (book.Status == "available")
            {
                return Result.Failure("Книга вільна. Використовуйте бронювання.");
            }

            if (await _reservationRepo.ExistsAsync(bookId, userId))
            {
                return Result.Failure("Ви вже у черзі.");
            }

            var entry = new ReservationQueue
            {
                BookId = bookId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            };

            await _reservationRepo.AddAsync(entry);

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
            var queueResult = await GetQueueUsersAsync(bookId);

            if (queueResult.IsFailure)
            {
                return Result.Failure("Помилка при перевірці черги.");
            }

            var queueItem = await _reservationRepo.GetByUserAndBookAsync(userId, bookId);

            if (queueItem == null)
            {
                return Result.Failure("Користувача не знайдено в черзі.");
            }

            await _reservationRepo.DeleteAsync(queueItem);

            return Result.Success();
        }
    }
}