namespace LNUBookShare.Domain.Models
{
    public class ReviewSettings
    {
        public int MinRating { get; set; }
        public int MaxRating { get; set; }
        public int MaxCommentLength { get; set; }
    }
}