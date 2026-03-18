using LNUBookShare.Domain.Entities;
namespace LNUBookShare.Web.Models
{
    public class BookSearchViewModel
    {
        public string Title { get; set; } = "Пошук книг";
        public string? SearchQuery { get; set; }
        public IEnumerable<Book> Books { get; set; } = new List<Book>();
        public int TotalCount
        {
            get
            {
                return Books.Count();
            }
        }

        public string SearchBy { get; set; } = "title";
    }
}
