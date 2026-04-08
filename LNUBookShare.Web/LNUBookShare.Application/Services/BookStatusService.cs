using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Services
{
    public class BookStatusService : IBookStatusService
    {
        private readonly IProfileService _profileService;
        private readonly IReservationService _reservationService;
        private readonly INotificationService _notificationService;

        public BookStatusService(
            IProfileService profileService,
            IReservationService reservationService,
            INotificationService notificationService)
        {
            _profileService = profileService;
            _reservationService = reservationService;
            _notificationService = notificationService;
        }

        public async Task<Result> IssueBookAsync(int bookId, int ownerId)
        {
            var booksResult = await _profileService.GetUserBooksAsync(ownerId);
            var book = booksResult.IsSuccess ? booksResult.Value.FirstOrDefault(b => b.BookId == bookId) : null;

            if (book == null)
            {
                return Result.Failure("Книгу не знайдено.");
            }

            book.Status = "borrowed";
            await _profileService.UpdateBookAsync(book);

            return Result.Success();
        }

        public async Task<Result> ConfirmReturnAsync(int bookId, int ownerId)
        {
            var booksResult = await _profileService.GetUserBooksAsync(ownerId);
            var book = booksResult.IsSuccess ? booksResult.Value.FirstOrDefault(b => b.BookId == bookId) : null;

            if (book == null)
            {
                return Result.Failure("Книгу не знайдено.");
            }

            var queueResult = await _reservationService.GetQueueUsersAsync(bookId);
            var queueUsers = queueResult.IsSuccess ? queueResult.Value.ToList() : new List<User>();

            if (queueUsers.Any())
            {
                var currentUser = queueUsers.First();
                await _reservationService.LeaveQueueAsync(bookId, currentUser.Id);
                queueUsers.RemoveAt(0);
            }

            if (queueUsers.Any())
            {
                book.Status = "reserved";
                var nextUser = queueUsers.First();

                await _notificationService.CreateNotificationAsync(
                    nextUser.Id,
                    $"Книга '{book.Title}' повернулася до власника і тепер автоматично заброньована за вами! Зв'яжіться з власником.",
                    book.BookId);
            }
            else
            {
                book.Status = "available";
            }

            await _profileService.UpdateBookAsync(book);
            return Result.Success();
        }
    }
}