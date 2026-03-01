using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Tiến độ học từng bài của một học viên.
    /// </summary>
    public class LessonProgress
    {
        [Key]
        public Guid ProgressId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        [Required]
        public int LessonId { get; set; }

        public bool IsCompleted { get; set; } = false;

        /// <summary>
        /// Số giây video đã xem (để resume tiếp tục).
        /// </summary>
        public int WatchedSeconds { get; set; } = 0;

        public DateTime? CompletedAt { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;

        [ForeignKey(nameof(LessonId))]
        public Lesson Lesson { get; set; } = default!;
    }
}

