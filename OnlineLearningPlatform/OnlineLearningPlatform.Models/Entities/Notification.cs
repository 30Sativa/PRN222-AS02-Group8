using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Thông báo gửi tới từng user (đơn hoàn thành, bài mới, hệ thống...).
    /// </summary>
    public class Notification
    {
        [Key]
        public int NotificationId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        [MaxLength(255)]
        public string Content { get; set; } = default!;

        [MaxLength(300)]
        public string? TargetUrl { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;
    }

    /// <summary>
    /// Loại thông báo gửi cho user.
    /// </summary>
    public enum NotificationType
    {
        OrderCompleted,
        RefundApproved,
        NewLesson,
        AssignmentGraded,
        System
    }
}

