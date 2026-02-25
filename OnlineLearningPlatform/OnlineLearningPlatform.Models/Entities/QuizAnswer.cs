using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Câu trả lời của học viên cho từng câu hỏi trong một lần làm Quiz.
    /// </summary>
    public class QuizAnswer
    {
        [Key]
        public Guid AnswerId { get; set; }

        [Required]
        public Guid AttemptId { get; set; }

        [Required]
        public int QuestionId { get; set; }

        [Required]
        [MaxLength(200)]
        public string UserAnswer { get; set; } = default!;

        public bool IsCorrect { get; set; }

        [ForeignKey(nameof(AttemptId))]
        public QuizAttempt QuizAttempt { get; set; } = default!;

        [ForeignKey(nameof(QuestionId))]
        public Question Question { get; set; } = default!;
    }
}

