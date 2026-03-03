using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;

        public ChatHub(IChatService chatService)
        {
            _chatService = chatService;
        }

        /// <summary>Client join vào group của conversation để nhận tin nhắn real-time.</summary>
        public async Task JoinConversation(Guid conversationId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return;

            if (!await _chatService.CanAccessConversationAsync(conversationId, userId)) return;

            await Groups.AddToGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        /// <summary>Client gửi tin nhắn — lưu DB + broadcast cho group.</summary>
        public async Task SendMessage(Guid conversationId, string content)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null || string.IsNullOrWhiteSpace(content)) return;

            if (!await _chatService.CanAccessConversationAsync(conversationId, userId)) return;

            var msg = await _chatService.SendMessageAsync(conversationId, userId, content);

            // Broadcast cho tất cả trong conversation group (kể cả người gửi)
            await Clients.Group(conversationId.ToString()).SendAsync("ReceiveMessage", new
            {
                messageId   = msg.MessageId,
                conversationId = msg.ConversationId,
                senderId    = msg.SenderId,
                senderName  = msg.SenderName,
                content     = msg.Content,
                sentAt      = msg.SentAt.ToString("o"),
                isRead      = false
            });
        }

        /// <summary>Client rời group khi đóng cửa sổ chat.</summary>
        public async Task LeaveConversation(Guid conversationId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, conversationId.ToString());
        }

        /// <summary>Đánh dấu đã đọc từ phía người nhận.</summary>
        public async Task MarkRead(Guid conversationId)
        {
            var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return;

            await _chatService.MarkReadAsync(conversationId, userId);
            await Clients.Group(conversationId.ToString()).SendAsync("MessagesRead", conversationId);
        }
    }
}
