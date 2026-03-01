using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Chương (Section) trong một khóa học, chứa nhiều Lesson.
    /// </summary>
    public class Section
    {
        [Key]
        public int SectionId { get; set; }

        [Required]
        public Guid CourseId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = default!;

        /// <summary>
        /// Thứ tự chương trong khóa học.
        /// </summary>
        public int OrderIndex { get; set; } = 0;

        [ForeignKey(nameof(CourseId))]
        public Course Course { get; set; } = default!;

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
    }
}

