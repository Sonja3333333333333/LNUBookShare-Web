namespace LNUBookShare.Application.Models
{
    public class ConversationDto
    {
        public int InterlocutorId { get; set; }
        public string InterlocutorName { get; set; } = null!;
        public string? InterlocutorAvatar { get; set; }
        public string LastMessage { get; set; } = null!;
        public DateTime LastMessageTime { get; set; }
    }
}