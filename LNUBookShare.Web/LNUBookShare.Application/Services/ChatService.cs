using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Models;
using LNUBookShare.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace LNUBookShare.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepo;
        private readonly IChatNotificationService _notificationService;
        private readonly ILogger<ChatService> _logger;

        public ChatService(
            IChatRepository chatRepo,
            IChatNotificationService notificationService,
            ILogger<ChatService> logger)
        {
            _chatRepo = chatRepo;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<Result> SendMessageAsync(int senderId, int receiverId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return Result.Failure("Повідомлення не може бути порожнім.");
            }

            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Content = content.Trim(),
                SentAt = DateTime.UtcNow,
            };

            try
            {
                await _chatRepo.AddAsync(message);
                await _notificationService.NotifyNewMessageAsync(receiverId, senderId, message.Content);
                return Result.Success();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Помилка при відправці повідомлення");
                return Result.Failure("Не вдалося відправити повідомлення.");
            }
        }

        public async Task<Result<IEnumerable<ChatMessage>>> GetChatHistoryAsync(int userId1, int userId2)
        {
            var messages = await _chatRepo.GetMessagesAsync(userId1, userId2);
            return Result<IEnumerable<ChatMessage>>.Success(messages);
        }

        public async Task<Result<IEnumerable<ConversationDto>>> GetConversationsAsync(int userId)
        {
            var lastMessages = await _chatRepo.GetUserConversationsAsync(userId);

            var conversations = lastMessages.Select(m =>
            {
                var interlocutor = m.SenderId == userId ? m.Receiver : m.Sender;
                return new ConversationDto
                {
                    InterlocutorId = interlocutor.Id,
                    InterlocutorName = $"{interlocutor.FirstName} {interlocutor.LastName}",
                    InterlocutorAvatar = interlocutor.Avatar?.ImagePath,
                    LastMessage = m.Content,
                    LastMessageTime = m.SentAt,
                };
            });

            return Result<IEnumerable<ConversationDto>>.Success(conversations);
        }
    }
}