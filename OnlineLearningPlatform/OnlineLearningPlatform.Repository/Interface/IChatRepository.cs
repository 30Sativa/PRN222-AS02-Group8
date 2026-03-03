using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IChatRepository
    {
        Task<ChatConversation?> GetConversationAsync(string studentId, string teacherId, Guid courseId);
        Task<ChatConversation> CreateConversationAsync(ChatConversation conversation);
        Task<List<ChatConversation>> GetConversationsByStudentAsync(string studentId);
        Task<List<ChatConversation>> GetConversationsByTeacherAsync(string teacherId);
        Task<List<ChatMessage>> GetMessagesAsync(Guid conversationId);
        Task<ChatMessage> SaveMessageAsync(ChatMessage message);
        Task MarkMessagesReadAsync(Guid conversationId, string readerId);
        Task<ChatConversation?> GetConversationByIdAsync(Guid conversationId);
        Task UpdateConversationTimestampAsync(Guid conversationId);
    }
}
