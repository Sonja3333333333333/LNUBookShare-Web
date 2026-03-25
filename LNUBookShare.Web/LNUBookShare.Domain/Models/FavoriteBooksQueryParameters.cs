namespace LNUBookShare.Domain.Models
{
    public class FavoriteBooksQueryParameters
    {
        public string? SortBy { get; set; }
        public bool IsDescending { get; set; }
        public string? Status { get; set; }
    }
}