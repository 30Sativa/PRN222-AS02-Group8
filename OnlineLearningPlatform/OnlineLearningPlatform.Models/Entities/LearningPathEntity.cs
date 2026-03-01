using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Lộ trình học gồm nhiều khóa (ví dụ: Lộ trình Web Fullstack).
    /// </summary>
    public class LearningPath
    {
        [Key]
        public int LearningPathId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = default!;

        [Required]
        [MaxLength(220)]
        public string Slug { get; set; } = default!;

        public string? Description { get; set; }

        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        [Required]
        public string CreatedById { get; set; } = default!;

        public bool IsPublished { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        [ForeignKey(nameof(CreatedById))]
        public ApplicationUser CreatedBy { get; set; } = default!;

        public ICollection<LearningPathCourse> LearningPathCourses { get; set; } = new List<LearningPathCourse>();
    }
}

