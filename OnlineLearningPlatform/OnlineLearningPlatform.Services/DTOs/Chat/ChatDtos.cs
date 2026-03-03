namespace OnlineLearningPlatform.Services.DTOs.Chat
{
    public class ChatMessageDto
    {
        public Guid MessageId { get; set; }
        public Guid ConversationId { get; set; }
        public string SenderId { get; set; } = default!;
        public string SenderName { get; set; } = default!;
        public string Content { get; set; } = default!;
        public DateTime SentAt { get; set; }
        public bool IsRead { get; set; }
        public bool IsMine { get; set; }
    }

    public class ChatConversationDto
    {
        public Guid ConversationId { get; set; }
        public string OtherUserId { get; set; } = default!;
        public string OtherUserName { get; set; } = default!;
        public Guid CourseId { get; set; }
        public string CourseTitle { get; set; } = default!;
        public string? LastMessage { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int UnreadCount { get; set; }
    }

    public class StartConversationRequest
    {
        public string TeacherId { get; set; } = default!;
        public Guid CourseId { get; set; }
    }
}
