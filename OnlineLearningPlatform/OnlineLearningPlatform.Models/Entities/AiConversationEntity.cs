using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Một hội thoại giữa user và AI trong hệ thống.
    /// </summary>
    public class AiConversation
    {
        [Key]
        public Guid ConversationId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        public Guid? CourseId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;

        public ICollection<AiMessage> AiMessages { get; set; } = new List<AiMessage>();
    }
}

