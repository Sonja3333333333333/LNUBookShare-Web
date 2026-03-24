using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Web.Models
{
    public class BookDetailsViewModel
    {
        // Основна інформація про книгу
        public int BookId { get; set; }

        public string? Title { get; set; }

        public string? Author { get; set; }

        public User? Owner { get; set; }

        public string? Status { get; set; }

        public Category? Category { get; set; }

        public string? Language { get; set; }

        public string? Publisher { get; set; }

        public string? Isbn { get; set; }

        public Image? Cover { get; set; }

        // --- ТАСКА #57: ВІДГУКИ ТА РЕЙТИНГ ---

        // Список відгуків, які ми отримаємо з ReviewService
        public IEnumerable<BookReview> BookReviews { get; set; } = new List<BookReview>();

        // Середній рейтинг, який порахує сервіс (наприклад, 4.5)
        public double AverageRating { get; set; }

        // Поля для форми додавання нового відгуку (зручно мати їх тут)
        public int NewRating { get; set; }

        public string? NewComment { get; set; }

        // --- ДОДАТКОВО ---

        // Список ID книг, які користувач додав в обране (для сердечка)
        public HashSet<int> FavoritedBookIds { get; set; } = new HashSet<int>();
    }
}