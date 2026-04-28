namespace LNUBookShare.Domain.Entities
{
    public class Report
    {
        public int Id { get; set; }

        // Хто скаржиться
        public int SenderId { get; set; }
        public User Sender { get; set; } = null!;

        // На кого скаржаться
        public int ReportedUserId { get; set; }
        public User ReportedUser { get; set; } = null!;

        // Текст скарги
        public string Context { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}