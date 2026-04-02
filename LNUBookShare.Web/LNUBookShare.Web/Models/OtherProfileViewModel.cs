using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Web.Models
{
    public class OtherProfileViewModel
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public string? Faculty { get; set; }
        public Image? Avatar { get; set; }

        public IEnumerable<Book> Books { get; set; } = new List<Book>();
        public string SortBy { get; set; } = "title";
        public string StatusFilter { get; set; } = "all";
        public HashSet<int> FavoritedBookIds { get; set; } = new HashSet<int>();
    }
}
