using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Một message trong hội thoại AI (User hoặc Assistant).
    /// </summary>
    public class AiMessage
    {
        [Key]
        public Guid MessageId { get; set; }

        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        public AiMessageRole Role { get; set; }

        [Required]
        public string Content { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(ConversationId))]
        public AiConversation AiConversation { get; set; } = default!;
    }

    /// <summary>
    /// Vai trò của message trong hội thoại AI (người dùng hay trợ lý).
    /// </summary>
    public enum AiMessageRole
    {
        User,
        Assistant
    }
}

