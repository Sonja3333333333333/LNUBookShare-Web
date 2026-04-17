using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Models;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LNUBookShare.Application.Services
{
    public class ChatService : IChatService
    {
        private readonly IChatRepository _chatRepo;
        private readonly IChatNotificationService _notificationService;
        private readonly ILogger<ChatService> _logger;
        private readonly ChatSettings _settings;

        public ChatService(
            IChatRepository chatRepo,
            IChatNotificationService notificationService,
            ILogger<ChatService> logger,
            IOptions<ChatSettings> options)
        {
            _chatRepo = chatRepo;
            _notificationService = notificationService;
            _logger = logger;
            _settings = options.Value;
        }

        public async Task<Result> SendMessageAsync(int senderId, int receiverId, string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                _logger.LogWarning("Користувач {SenderId} спробував надіслати порожнє повідомлення користувачу {ReceiverId}.", senderId, receiverId);
                return Result.Failure("Повідомлення не може бути порожнім.");
            }

            var trimmedContent = content.Trim();

            if (trimmedContent.Length > _settings.MaxMessageLength)
            {
                _logger.LogWarning("Користувач {UserId} спробував надіслати повідомлення, що перевищує ліміт ({Length} символів).", senderId, trimmedContent.Length);
                return Result.Failure($"Повідомлення занадто довге. Максимальна кількість символів: {_settings.MaxMessageLength}.");
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

                _logger.LogInformation("Повідомлення від {SenderId} до {ReceiverId} успішно надіслано.", senderId, receiverId);
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
            _logger.LogInformation("Отримання історії чату між користувачами {UserId1} та {UserId2}", userId1, userId2);
            var messages = await _chatRepo.GetMessagesAsync(userId1, userId2);
            return Result<IEnumerable<ChatMessage>>.Success(messages);
        }

        public async Task<Result<IEnumerable<ConversationDto>>> GetConversationsAsync(int userId)
        {
            _logger.LogInformation("Отримання списку активних діалогів для користувача {UserId}", userId);

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

        public async Task<Result> DeleteConversationAsync(int currentUserId, int interlocutorId)
        {
            if (currentUserId == interlocutorId)
            {
                _logger.LogWarning("Спроба видалення діалогу з самим собою: UserId {UserId}", currentUserId);
                return Result.Failure("Неможливо виконати операцію для однакових ID.");
            }

            var messages = await _chatRepo.GetMessagesAsync(currentUserId, interlocutorId);

            if (messages == null || !messages.Any())
            {
                _logger.LogInformation("Діалог між {UserId} та {InterlocutorId} не знайдений або вже порожній", currentUserId, interlocutorId);
                return Result.Failure("Діалог уже порожній або не існує.");
            }

            await _chatRepo.DeleteConversationAsync(currentUserId, interlocutorId);

            _logger.LogInformation("Чат між {UserId} та {InterlocutorId} успішно позначено як видалений", currentUserId, interlocutorId);

            return Result.Success();
        }
    }
}