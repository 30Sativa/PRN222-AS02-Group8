using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Cuộc hội thoại chat 1-1 giữa 1 Student và 1 Teacher về 1 Course.
    /// </summary>
    public class ChatConversation
    {
        [Key]
        public Guid ConversationId { get; set; } = Guid.NewGuid();

        [Required]
        public string StudentId { get; set; } = default!;

        [Required]
        public string TeacherId { get; set; } = default!;

        [Required]
        public Guid CourseId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [ForeignKey(nameof(StudentId))]
        public Identity.ApplicationUser Student { get; set; } = default!;

        [ForeignKey(nameof(TeacherId))]
        public Identity.ApplicationUser Teacher { get; set; } = default!;

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = default!;

        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
