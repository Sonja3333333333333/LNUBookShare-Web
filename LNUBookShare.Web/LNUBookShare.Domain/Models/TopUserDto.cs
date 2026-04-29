namespace LNUBookShare.Domain.Models
{
    public class TopUserDto
    {
        public int UserId { get; set; }
        public string FullName { get; set; } = null!;
        public int? AvatarId { get; set; }
        public string? AvatarPath { get; set; }
        public int AddedBooksCount { get; set; }
    }
}