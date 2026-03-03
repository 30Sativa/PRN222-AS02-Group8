using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Một tin nhắn trong cuộc hội thoại chat.
    /// </summary>
    public class ChatMessage
    {
        [Key]
        public Guid MessageId { get; set; } = Guid.NewGuid();

        [Required]
        public Guid ConversationId { get; set; }

        [Required]
        public string SenderId { get; set; } = default!;

        [Required]
        public string Content { get; set; } = default!;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        // Navigation
        [ForeignKey(nameof(ConversationId))]
        public ChatConversation Conversation { get; set; } = default!;

        [ForeignKey(nameof(SenderId))]
        public Identity.ApplicationUser Sender { get; set; } = default!;
    }
}
