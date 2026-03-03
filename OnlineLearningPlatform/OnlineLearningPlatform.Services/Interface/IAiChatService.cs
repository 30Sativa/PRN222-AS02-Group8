using OnlineLearningPlatform.Services.DTOs.AiChat;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IAiChatService
    {
        /// <summary>
        /// Gửi tin nhắn tới AI và nhận phản hồi. Nếu câu hỏi liên quan đến dữ liệu,
        /// AI sẽ tự sinh SQL (SELECT only) và thực thi để trả lời chính xác.
        /// </summary>
        Task<AiChatResponseDto> ChatAsync(string userId, string userMessage, Guid? conversationId = null);

        /// <summary>
        /// Lấy lịch sử hội thoại của user.
        /// </summary>
        Task<List<AiMessageDto>> GetHistoryAsync(string userId, Guid conversationId);

        /// <summary>
        /// Lấy danh sách hội thoại của user.
        /// </summary>
        Task<List<AiConversationSummaryDto>> GetConversationsAsync(string userId);

        /// <summary>
        /// Tạo hội thoại mới.
        /// </summary>
        Task<Guid> CreateConversationAsync(string userId);
    }
}
