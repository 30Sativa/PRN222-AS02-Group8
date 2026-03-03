using OnlineLearningPlatform.Services.DTOs.Chat;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IChatService
    {
        /// <summary>Lấy hoặc tạo mới conversation giữa student và teacher trong 1 course.</summary>
        Task<Guid> GetOrCreateConversationAsync(string studentId, string teacherId, Guid courseId);

        /// <summary>Lấy danh sách conversations của user (phân theo role Student/Teacher).</summary>
        Task<List<ChatConversationDto>> GetMyConversationsAsync(string userId, string role);

        /// <summary>Lấy lịch sử tin nhắn của 1 conversation (requester phải là thành viên).</summary>
        Task<List<ChatMessageDto>> GetMessagesAsync(Guid conversationId, string requesterId);

        /// <summary>Lưu tin nhắn mới và trả về DTO.</summary>
        Task<ChatMessageDto> SendMessageAsync(Guid conversationId, string senderId, string content);

        /// <summary>Đánh dấu đã đọc tất cả tin nhắn trong conversation.</summary>
        Task MarkReadAsync(Guid conversationId, string readerId);

        /// <summary>Kiểm tra user có quyền truy cập conversation này không.</summary>
        Task<bool> CanAccessConversationAsync(Guid conversationId, string userId);

        /// <summary>Lấy danh sách Teacher+Course mà Student có thể bắt đầu chat.</summary>
        Task<List<ChatConversationDto>> GetAvailableTeachersAsync(string studentId);
    }
}
