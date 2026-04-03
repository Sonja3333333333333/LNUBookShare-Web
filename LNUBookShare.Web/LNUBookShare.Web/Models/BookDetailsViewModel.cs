using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Web.Models
{
    public class BookDetailsViewModel
    {
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

        // --- ВІДГУКИ ---
        public IEnumerable<BookReview> BookReviews { get; set; } = new List<BookReview>();
        public double AverageRating { get; set; }

        // --- ЧЕРГА (НОВЕ) ---
        public bool IsInQueue { get; set; }
        public int QueuePosition { get; set; }

        // --- ОБРАНЕ ---
        public HashSet<int> FavoritedBookIds { get; set; } = new HashSet<int>();

        public bool HasUserReviewed { get; set; }
    }
}