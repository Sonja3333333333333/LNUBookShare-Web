using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IChatRepository
    {
        Task AddAsync(ChatMessage message);
        Task<IEnumerable<ChatMessage>> GetMessagesAsync(int userId1, int userId2);
        Task<IEnumerable<ChatMessage>> GetUserConversationsAsync(int userId);
        Task DeleteConversationAsync(int currentUserId, int interlocutorId);
    }
}