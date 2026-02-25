using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Thông tin chính của một khóa học.
    /// </summary>
    public class Course
    {
        [Key]
        public Guid CourseId { get; set; }

        /// <summary>
        /// Mã khóa học ngắn gọn, duy nhất (VD: OOP1, REACT101).
        /// </summary>
        [Required]
        [MaxLength(20)]
        public string CourseCode { get; set; } = default!;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = default!;

        /// <summary>
        /// Slug thân thiện dùng cho URL (vd: lap-trinh-oop1).
        /// </summary>
        [Required]
        [MaxLength(220)]
        public string Slug { get; set; } = default!;

        /// <summary>
        /// Mô tả chi tiết (có thể là HTML hoặc Markdown).
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Id giảng viên tạo khóa học.
        /// </summary>
        [Required]
        public string TeacherId { get; set; } = default!;

        public int? CategoryId { get; set; }

        /// <summary>
        /// Giá gốc của khóa học (0 = miễn phí).
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        /// <summary>
        /// Giá sau giảm (nếu có khuyến mãi).
        /// </summary>
        [Column(TypeName = "decimal(18,2)")]
        public decimal? DiscountPrice { get; set; }

        /// <summary>
        /// Đường dẫn ảnh thumbnail (file lưu trên server/S3).
        /// </summary>
        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        public CourseLevel Level { get; set; } = CourseLevel.Beginner;

        /// <summary>
        /// Mã ngôn ngữ (vd: vi, en...).
        /// </summary>
        [MaxLength(10)]
        public string Language { get; set; } = "vi";

        /// <summary>
        /// Tổng thời lượng video (tính bằng giây). Tự tính từ các Lesson.
        /// </summary>
        public int TotalDuration { get; set; } = 0;

        public CourseStatus Status { get; set; } = CourseStatus.Pending;

        /// <summary>
        /// Lý do bị từ chối khi duyệt khóa.
        /// </summary>
        public string? RejectionReason { get; set; }

        /// <summary>
        /// Đánh dấu là khóa học nổi bật để ưu tiên hiển thị.
        /// </summary>
        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Soft delete: true = khóa học đã bị ẩn/xóa logic.
        /// </summary>
        public bool IsDeleted { get; set; } = false;

        // Navigation
        [ForeignKey(nameof(TeacherId))]
        public ApplicationUser Teacher { get; set; } = default!;

        [ForeignKey(nameof(CategoryId))]
        public Category? Category { get; set; }

        public ICollection<Section> Sections { get; set; } = new List<Section>();

        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();

        public ICollection<Review> Reviews { get; set; } = new List<Review>();

        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

        public ICollection<Announcement> Announcements { get; set; } = new List<Announcement>();

        public ICollection<DiscussionTopic> DiscussionTopics { get; set; } = new List<DiscussionTopic>();

        public ICollection<LearningPathCourse> LearningPathCourses { get; set; } = new List<LearningPathCourse>();
    }

    /// <summary>
    /// Trạng thái duyệt và xuất bản khóa học.
    /// </summary>
    public enum CourseStatus
    {
        Pending,
        Published,
        Rejected
    }

    /// <summary>
    /// Cấp độ khó của khóa học.
    /// </summary>
    public enum CourseLevel
    {
        Beginner,
        Intermediate,
        Advanced
    }
}

