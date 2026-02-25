using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Comment/reply trong một topic thảo luận (hỗ trợ reply lồng nhau).
    /// </summary>
    public class DiscussionReply
    {
        [Key]
        public Guid ReplyId { get; set; }

        [Required]
        public Guid TopicId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        [Required]
        public string Content { get; set; } = default!;

        /// <summary>
        /// Reply cha (để tạo cấu trúc comment lồng nhau).
        /// </summary>
        public Guid? ParentReplyId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        [ForeignKey(nameof(TopicId))]
        public DiscussionTopic DiscussionTopic { get; set; } = default!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;
    }
}

