using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Bài kiểm tra (Quiz) gắn với một Lesson.
    /// </summary>
    public class Quiz
    {
        [Key]
        public int QuizId { get; set; }

        [Required]
        public int LessonId { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = default!;

        /// <summary>
        /// Điểm phần trăm tối thiểu để được coi là qua bài.
        /// </summary>
        public double PassingScore { get; set; } = 70;

        /// <summary>
        /// Giới hạn thời gian (phút). Null = không giới hạn.
        /// </summary>
        public int? TimeLimitMinutes { get; set; }

        [ForeignKey(nameof(LessonId))]
        public Lesson Lesson { get; set; } = default!;

        public ICollection<Question> Questions { get; set; } = new List<Question>();

        public ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
    }
}

