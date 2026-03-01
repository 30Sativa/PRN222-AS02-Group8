using System;

namespace OnlineLearningPlatform.Services.DTOs.Discussion
{
    public class DiscussionReplyDto
    {
        public Guid ReplyId { get; set; }
        public Guid TopicId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string UserRole { get; set; } = "Student"; // "Teacher" | "Student"
        public string Content { get; set; } = string.Empty;
        public Guid? ParentReplyId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
