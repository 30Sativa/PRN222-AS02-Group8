using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Thông báo chung cho khóa học hoặc toàn hệ thống (giống announcement).
    /// </summary>
    public class Announcement
    {
        [Key]
        public int AnnouncementId { get; set; }

        public Guid? CourseId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = default!;

        [Required]
        public string Content { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(CourseId))]
        public Course? Course { get; set; }
    }
}

