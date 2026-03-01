using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Chủ đề thảo luận trong khóa học (có thể gắn với 1 Lesson cụ thể).
    /// </summary>
    public class DiscussionTopic
    {
        [Key]
        public Guid TopicId { get; set; }

        [Required]
        public Guid CourseId { get; set; }

        public int? LessonId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = default!;

        [Required]
        public string CreatedBy { get; set; } = default!;

        public bool IsPinned { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsDeleted { get; set; } = false;

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = default!;

        [ForeignKey(nameof(CreatedBy))]
        public ApplicationUser Creator { get; set; } = default!;

        public ICollection<DiscussionReply> DiscussionReplies { get; set; } = new List<DiscussionReply>();
    }
}

