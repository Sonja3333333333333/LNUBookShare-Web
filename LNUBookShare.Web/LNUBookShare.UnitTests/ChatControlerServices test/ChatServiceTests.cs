using LNUBookShare.Application.Common;
using LNUBookShare.Application.Interfaces;
using LNUBookShare.Application.Models;
using LNUBookShare.Application.Services;
using LNUBookShare.Domain.Entities;
using LNUBookShare.Domain.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace LNUBookShare.UnitTests.Services
{
    public class ChatServiceTests
    {
        private readonly Mock<IChatRepository> _chatRepoMock;
        private readonly Mock<IChatNotificationService> _notificationMock;
        private readonly Mock<ILogger<ChatService>> _loggerMock;
        private readonly ChatService _chatService;

        public ChatServiceTests()
        {
            _chatRepoMock = new Mock<IChatRepository>();
            _notificationMock = new Mock<IChatNotificationService>();
            _loggerMock = new Mock<ILogger<ChatService>>();

            var chatSettings = new ChatSettings { MaxMessageLength = 1000 };

            var optionsWrapper = Options.Create(chatSettings);

            _chatService = new ChatService(
                _chatRepoMock.Object,
                _notificationMock.Object,
                _loggerMock.Object,
                optionsWrapper);
        }

        [Fact]
        public async Task SendMessageAsync_ValidData_ReturnsSuccess()
        {
            // Arrange
            int sId = 1, rId = 2;
            string txt = "Привіт!";

            // Act
            var res = await _chatService.SendMessageAsync(sId, rId, txt);

            // Assert
            Assert.True(res.IsSuccess);
            _chatRepoMock.Verify(r => r.AddAsync(It.IsAny<ChatMessage>()), Times.Once);
            _notificationMock.Verify(n => n.NotifyNewMessageAsync(rId, sId, txt), Times.Once);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task SendMessageAsync_InvalidContent_ReturnsFailure(string? content)
        {
            // Act
            var res = await _chatService.SendMessageAsync(1, 2, content!);

            // Assert
            Assert.False(res.IsSuccess);
            Assert.Equal("Повідомлення не може бути порожнім.", res.Error);
        }

        [Fact]
        public async Task SendMessageAsync_WhenDbFails_ReturnsFailure()
        {
            // Arrange
            _chatRepoMock.Setup(r => r.AddAsync(It.IsAny<ChatMessage>()))
                         .ThrowsAsync(new System.Exception("Neon error"));

            // Act
            var res = await _chatService.SendMessageAsync(1, 2, "Test");

            // Assert
            Assert.False(res.IsSuccess);
            Assert.Contains("Не вдалося", res.Error);
        }


        // Тестування видалення діалогу



        [Fact]
        public async Task DeleteConversationAsync_ShouldReturnFailure_WhenIdsAreEqual()
        {
            // Arrange
            int userId = 1;

            // Act
            var result = await _chatService.DeleteConversationAsync(userId, userId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Неможливо виконати операцію для однакових ID.", result.Error);
            _chatRepoMock.Verify(r => r.DeleteConversationAsync(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task DeleteConversationAsync_ShouldReturnFailure_WhenChatIsEmpty()
        {
            // Arrange: чат порожній 
            int uId = 1, iId = 2;
            _chatRepoMock.Setup(r => r.GetMessagesAsync(uId, iId))
                         .ReturnsAsync(new List<ChatMessage>());

            // Act
            var result = await _chatService.DeleteConversationAsync(uId, iId);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Діалог уже порожній або не існує.", result.Error);
        }

        [Fact]
        public async Task DeleteConversationAsync_ShouldReturnSuccess_WhenMessagesExist()
        {
            // Arrange: у чаті є повідомлення
            int uId = 1, iId = 2;
            var messages = new List<ChatMessage> { new ChatMessage { Content = "Test message" } };

            _chatRepoMock.Setup(r => r.GetMessagesAsync(uId, iId))
                         .ReturnsAsync(messages);

            // Act
            var result = await _chatService.DeleteConversationAsync(uId, iId);

            // Assert
            Assert.True(result.IsSuccess);
            _chatRepoMock.Verify(r => r.DeleteConversationAsync(uId, iId), Times.Once);
        }
    }
}