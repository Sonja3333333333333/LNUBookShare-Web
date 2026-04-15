using LNUBookShare.Application.Interfaces;
using LNUBookShare.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LNUBookShare.Infrastructure.Repositories
{
    public class ChatRepository : IChatRepository
    {
        private readonly AppDbContext _context;

        public ChatRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(ChatMessage message)
        {
            await _context.ChatMessages.AddAsync(message);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(int userId1, int userId2)
        {
            return await _context.ChatMessages
                .Where(m => (m.SenderId == userId1 && m.ReceiverId == userId2 && !m.IsDeletedBySender) ||
                            (m.SenderId == userId2 && m.ReceiverId == userId1 && !m.IsDeletedBySender))
                .OrderBy(m => m.SentAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ChatMessage>> GetUserConversationsAsync(int userId)
        {
            var allMessages = await _context.ChatMessages
                .Include(m => m.Sender).ThenInclude(u => u.Avatar)
                .Include(m => m.Receiver).ThenInclude(u => u.Avatar) // Фільтруємо лише ті повідомлення, які користувач ще не видалив "у себе"
                .Where(m => (m.SenderId == userId && !m.IsDeletedBySender) ||
                            (m.ReceiverId == userId && !m.IsDeletedByReceiver))
                .OrderByDescending(m => m.SentAt)
                .ToListAsync();

            return allMessages
                .GroupBy(m => m.SenderId == userId ? m.ReceiverId : m.SenderId)
                .Select(g => g.First())
                .ToList();
        }

        public async Task DeleteConversationAsync(int currentUserId, int interlocutorId)
        {
            await _context.ChatMessages // видаляємо відправлені повідомлення для поточного користувача
                .Where(m => m.SenderId == currentUserId && m.ReceiverId == interlocutorId)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsDeletedBySender, true));

            await _context.ChatMessages // видаляємо отримані повідомлення для поточного користувача
                .Where(m => m.SenderId == interlocutorId && m.ReceiverId == currentUserId)
                .ExecuteUpdateAsync(s => s.SetProperty(m => m.IsDeletedByReceiver, true));
        }
    }
}