using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Lần nộp bài tập của học viên (upload file thật, không dùng link ngoài).
    /// </summary>
    public class AssignmentSubmission
    {
        [Key]
        public Guid SubmissionId { get; set; }

        [Required]
        public int AssignmentId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        /// <summary>
        /// Đường dẫn file nộp lưu trên server/S3 (không dùng Google Drive...).
        /// </summary>
        [Required]
        [MaxLength(500)]
        public string FileStoragePath { get; set; } = default!;

        /// <summary>
        /// Tên file gốc để hiển thị cho giáo viên.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string OriginalFileName { get; set; } = default!;

        /// <summary>
        /// Kích thước file (bytes), dùng để kiểm tra trước khi lưu.
        /// </summary>
        public long FileSizeBytes { get; set; }

        [MaxLength(500)]
        public string? Note { get; set; }

        public double? Score { get; set; }

        public string? Feedback { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime? GradedAt { get; set; }

        [ForeignKey(nameof(AssignmentId))]
        public Assignment Assignment { get; set; } = default!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;
    }
}

