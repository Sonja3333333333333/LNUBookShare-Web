using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces;

public interface IReviewRepository
{
    // Додати новий відгук у базу
    Task AddAsync(BookReview review);

    // Дістати всі відгуки для конкретної книги
    Task<IEnumerable<BookReview>> GetByBookIdAsync(int bookId);
}