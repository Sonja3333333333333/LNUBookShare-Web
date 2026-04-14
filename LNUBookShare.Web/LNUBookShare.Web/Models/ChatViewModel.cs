using LNUBookShare.Application.Models;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Web.Models
{
    public class ChatViewModel
    {
        // Список діалогів для лівої панелі
        public IEnumerable<ConversationDto> Conversations { get; set; } = new List<ConversationDto>();

        // Повідомлення поточного чату
        public IEnumerable<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        // Дані того, з ким говоримо
        public User? SelectedUser { get; set; }

        public int CurrentUserId { get; set; }
    }
}