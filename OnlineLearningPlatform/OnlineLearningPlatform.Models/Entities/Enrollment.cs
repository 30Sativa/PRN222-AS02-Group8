using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Ghi danh của học viên vào một khóa học.
    /// </summary>
    public class Enrollment
    {
        [Key]
        public Guid EnrollmentId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        [Required]
        public Guid CourseId { get; set; }

        /// <summary>
        /// Khóa ngoại đến OrderDetail (null nếu khóa free hoặc ghi danh thủ công).
        /// </summary>
        public int? OrderDetailId { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Lần cuối học viên truy cập khóa học (dùng cho last accessed tracking).
        /// </summary>
        public DateTime? LastAccessedAt { get; set; }

        /// <summary>
        /// Khi refund và thu hồi quyền học, IsActive sẽ là false.
        /// </summary>
        public bool IsActive { get; set; } = true;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = default!;

        [ForeignKey(nameof(OrderDetailId))]
        public OrderDetail? OrderDetail { get; set; }
    }
}

