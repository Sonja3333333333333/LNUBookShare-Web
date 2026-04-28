using LNUBookShare.Domain.Entities;

public class UserReport
{
    public int Id { get; set; }
    public int SenderId { get; set; }
    public User Sender { get; set; } = null!;

    public int ReportedUserId { get; set; }
    public User ReportedUser { get; set; } = null!;

    public ReportReason Reason { get; set; } // Випадаючий список
    public string Details { get; set; } = string.Empty; // Текстове поле

    public ReportStatus Status { get; set; } = ReportStatus.Pending;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}