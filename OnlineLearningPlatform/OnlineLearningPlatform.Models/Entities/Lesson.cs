using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Một bài học trong chương: có thể là video, bài đọc, quiz, hoặc bài tập.
    /// </summary>
    public class Lesson
    {
        [Key]
        public int LessonId { get; set; }

        [Required]
        public int SectionId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = default!;

        [Required]
        public LessonType LessonType { get; set; }

        /// <summary>
        /// Thứ tự bài trong section.
        /// </summary>
        public int OrderIndex { get; set; } = 0;

        /// <summary>
        /// Cho phép học viên xem preview trước khi mua hay không.
        /// </summary>
        public bool IsPreview { get; set; } = false;

        /// <summary>
        /// Soft delete cho bài học.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        // ==== Video fields (chỉ dùng khi LessonType == Video) ====

        /// <summary>
        /// Đường dẫn file video thực tế trên server/S3/Azure Blob.
        /// Không dùng link YouTube.
        /// </summary>
        [MaxLength(500)]
        public string? VideoStoragePath { get; set; }

        /// <summary>
        /// Tên file gốc mà giáo viên upload.
        /// </summary>
        [MaxLength(255)]
        public string? VideoOriginalFileName { get; set; }

        /// <summary>
        /// Thời lượng video (giây), đọc từ ffprobe sau khi xử lý.
        /// </summary>
        public int? VideoDurationSeconds { get; set; }

        /// <summary>
        /// Trạng thái xử lý video (upload/processing/ready...).
        /// </summary>
        public VideoStatus? VideoStatus { get; set; }

        // ==== Reading fields (chỉ dùng khi LessonType == Reading) ====

        /// <summary>
        /// Nội dung bài đọc (HTML hoặc Markdown).
        /// </summary>
        public string? Content { get; set; }

        /// <summary>
        /// Đường dẫn file PDF đính kèm cho bài Reading (không bắt buộc).
        /// </summary>
        [MaxLength(500)]
        public string? ReadingPdfStoragePath { get; set; }

        /// <summary>
        /// Tên file PDF gốc giáo viên upload.
        /// </summary>
        [MaxLength(255)]
        public string? ReadingPdfOriginalFileName { get; set; }

        // Navigation
        [ForeignKey(nameof(SectionId))]
        public Section Section { get; set; } = default!;

        public ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();

        public ICollection<Quiz> Quizzes { get; set; } = new List<Quiz>();

        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }

    /// <summary>
    /// Loại bài học trong một Section.
    /// </summary>
    public enum LessonType
    {
        Video,
        Reading,
        Quiz,
        Assignment
    }

    /// <summary>
    /// Trạng thái xử lý file video.
    /// </summary>
    public enum VideoStatus
    {
        Uploading,
        Processing,
        Ready,
        Failed
    }
}

