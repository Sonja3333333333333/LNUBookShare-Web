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
        public ICollection<BookReview> BookReviews { get; set; } = new List<BookReview>();
        public double AverageRating
        {
            get
            {
                return BookReviews.Any() ? BookReviews.Average(r => r.Rating) : 0.0;
            }
        }

        public Category? Category { get; set; }
        public string? Language { get; set; }
        public string? Publisher { get; set; }
        public string? Isbn { get; set; }
        public Image? Cover { get; set; }
        public HashSet<int> FavoritedBookIds { get; set; } = new HashSet<int>();
    }
}
