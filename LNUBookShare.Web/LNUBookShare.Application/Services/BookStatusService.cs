using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class BookStatusService : IBookStatusService
    {
        private readonly IProfileService _profileService;
        private readonly IReservationService _reservationService;
        private readonly INotificationService _notificationService;
        private readonly ILogger<BookStatusService> _logger;

        public BookStatusService(
            IProfileService profileService,
            IReservationService reservationService,
            INotificationService notificationService,
            ILogger<BookStatusService> logger)
        {
            _profileService = profileService;
            _reservationService = reservationService;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Result> IssueBookAsync(int bookId, int ownerId)
        {
            _logger.LogInformation("Спроба видачі книги з ID {BookId} користувачем {OwnerId}", bookId, ownerId);

            var booksResult = await _profileService.GetUserBooksAsync(ownerId);
            var book = booksResult.IsSuccess ? booksResult.Value.FirstOrDefault(b => b.BookId == bookId) : null;

            if (book == null)
            {
                _logger.LogWarning("Помилка видачі: Книгу {BookId} не знайдено серед списку книг користувача {OwnerId}", bookId, ownerId);
                return Result.Failure("Книгу не знайдено.");
            }

            book.Status = "borrowed";
            await _profileService.UpdateBookAsync(book);

            _logger.LogInformation("Книгу {BookId} успішно видано (статус змінено на 'borrowed')", bookId);
            return Result.Success();
        }

        public async Task<Result> ConfirmReturnAsync(int bookId, int ownerId)
        {
            _logger.LogInformation("Спроба підтвердити повернення книги з ID {BookId} користувачем {OwnerId}", bookId, ownerId);
            var booksResult = await _profileService.GetUserBooksAsync(ownerId);
            var book = booksResult.IsSuccess ? booksResult.Value.FirstOrDefault(b => b.BookId == bookId) : null;

            if (book == null)
            {
                _logger.LogWarning("Помилка повернення: Книгу {BookId} не знайдено серед списку книг користувача {OwnerId}", bookId, ownerId);
                return Result.Failure("Книгу не знайдено.");
            }

            var queueResult = await _reservationService.GetQueueUsersAsync(bookId);
            var queueUsers = queueResult.IsSuccess ? queueResult.Value.ToList() : new List<User>();

            if (queueUsers.Any())
            {
                var currentUser = queueUsers.First();
                await _reservationService.LeaveQueueAsync(bookId, currentUser.Id);
                queueUsers.RemoveAt(0);

                _logger.LogInformation("Поточного користувача {UserId} видалено з черги для книги {BookId}", currentUser.Id, bookId);
            }

            if (queueUsers.Any())
            {
                book.Status = "reserved";
                var nextUser = queueUsers.First();

                await _notificationService.CreateNotificationAsync(
                    nextUser.Id,
                    $"Книга '{book.Title}' повернулася до власника і тепер автоматично заброньована за вами! Зв'яжіться з власником.",
                    book.BookId);

                _logger.LogInformation("Книгу {BookId} автоматично заброньовано для наступного користувача в черзі {NextUserId}", bookId, nextUser.Id);
            }
            else
            {
                book.Status = "available";
                _logger.LogInformation("Черга порожня. Статус книги {BookId} змінено на 'available'", bookId);
            }

            await _profileService.UpdateBookAsync(book);

            _logger.LogInformation("Процес повернення книги {BookId} успішно завершено", bookId);
            return Result.Success();
        }
    }
}