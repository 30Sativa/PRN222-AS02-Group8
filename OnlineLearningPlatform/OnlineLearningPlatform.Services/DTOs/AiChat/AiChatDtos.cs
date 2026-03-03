namespace OnlineLearningPlatform.Services.DTOs.AiChat
{
    public class AiMessageDto
    {
        public Guid MessageId { get; set; }
        public string Role { get; set; } = "user"; // "user" | "assistant"
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public string? SqlQuery { get; set; }      // SQL đã chạy (nếu có)
        public string? QueryResult { get; set; }   // Kết quả SQL dạng text (nếu có)
    }

    public class AiChatResponseDto
    {
        public Guid ConversationId { get; set; }
        public AiMessageDto UserMessage { get; set; } = default!;
        public AiMessageDto AssistantMessage { get; set; } = default!;
        public bool UsedSql { get; set; }
        public string? SqlQuery { get; set; }
    }

    public class AiConversationSummaryDto
    {
        public Guid ConversationId { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }
}
