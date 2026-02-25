using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Đánh giá (rating + comment) của học viên cho khóa học.
    /// </summary>
    public class Review
    {
        [Key]
        public int ReviewId { get; set; }

        [Required]
        public Guid CourseId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        [Required]
        [Range(1, 5)]
        public int Rating { get; set; }

        [Required]
        public string Comment { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsDeleted { get; set; } = false;

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = default!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;
    }
}

