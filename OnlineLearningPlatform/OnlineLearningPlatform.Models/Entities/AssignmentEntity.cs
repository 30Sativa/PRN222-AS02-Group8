using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Bài tập gắn với một Lesson.
    /// </summary>
    public class Assignment
    {
        [Key]
        public int AssignmentId { get; set; }

        [Required]
        public int LessonId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = default!;

        /// <summary>
        /// Mô tả nội dung/yêu cầu bài tập (HTML/Markdown).
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// File đề bài đính kèm (pdf/zip...) lưu trên server/S3.
        /// </summary>
        [MaxLength(500)]
        public string? AttachmentStoragePath { get; set; }

        /// <summary>
        /// Chuỗi danh sách phần mở rộng cho phép (".zip,.pdf,.cs"...).
        /// </summary>
        [MaxLength(100)]
        public string? AllowedFileExtensions { get; set; }

        public int MaxFileSizeMB { get; set; } = 50;

        /// <summary>
        /// Deadline nộp bài (có thể null nếu không giới hạn).
        /// </summary>
        public DateTime? DueDate { get; set; }

        [ForeignKey(nameof(LessonId))]
        public Lesson Lesson { get; set; } = default!;

        public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();
    }
}

