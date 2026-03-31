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
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IReservationRepository reservationRepo,
            IBookRepository bookRepo,
            ILogger<ReservationService> logger)
        {
            _reservationRepo = reservationRepo;
            _bookRepo = bookRepo;
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

            // 1. Змінюємо статус книги
            book.Status = "reserved";
            await _bookRepo.UpdateAsync(book);

            // 2. Додаємо першого юзера в чергу
            var entry = new ReservationQueue
            {
                BookId = bookId,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
            };

            await _reservationRepo.AddAsync(entry);
            _logger.LogInformation("User {UserId} reserved book {BookId}", userId, bookId);

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
            return Result.Success();
        }

        public async Task<int> GetQueuePositionAsync(int bookId, int userId)
        {
            return await _reservationRepo.GetPositionAsync(bookId, userId);
        }

        public async Task<bool> IsUserInQueueAsync(int bookId, int userId)
        {
            return await _reservationRepo.ExistsAsync(bookId, userId);
        }
    }
}