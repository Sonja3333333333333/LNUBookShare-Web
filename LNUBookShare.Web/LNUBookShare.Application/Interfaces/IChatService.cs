using LNUBookShare.Application.Common;
using LNUBookShare.Application.Models;
using LNUBookShare.Domain.Entities;

namespace LNUBookShare.Application.Interfaces
{
    public interface IChatService
    {
        Task<Result> SendMessageAsync(int senderId, int receiverId, string content);
        Task<Result<IEnumerable<ChatMessage>>> GetChatHistoryAsync(int userId1, int userId2);
        Task<Result<IEnumerable<ConversationDto>>> GetConversationsAsync(int userId);
        Task<Result> DeleteConversationAsync(int currentUserId, int interlocutorId);
    }
}