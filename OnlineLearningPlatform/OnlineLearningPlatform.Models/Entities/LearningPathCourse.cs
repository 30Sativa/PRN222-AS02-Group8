using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Bảng trung gian giữa LearningPath và Course (nhiều-nhiều) + thứ tự.
    /// </summary>
    public class LearningPathCourse
    {
        [Key]
        public int LearningPathCourseId { get; set; }

        [Required]
        public int LearningPathId { get; set; }

        [Required]
        public Guid CourseId { get; set; }

        /// <summary>
        /// Thứ tự của khóa học trong lộ trình.
        /// </summary>
        public int OrderIndex { get; set; } = 0;

        [ForeignKey(nameof(LearningPathId))]
        public LearningPath LearningPath { get; set; } = default!;

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = default!;
    }
}

