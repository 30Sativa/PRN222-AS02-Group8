using OnlineLearningPlatform.Models.Entities.Identity;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Câu hỏi thuộc một Quiz.
    /// </summary>
    public class Question
    {
        [Key]
        public int QuestionId { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        public string Content { get; set; } = default!;

        /// <summary>
        /// Loại câu hỏi (trắc nghiệm, true/false, trả lời ngắn).
        /// </summary>
        [Required]
        public QuestionType QuestionType { get; set; }

        [Required]
        [MaxLength(200)]
        public string CorrectAnswer { get; set; } = default!;

        public int OrderIndex { get; set; } = 0;

        [ForeignKey(nameof(QuizId))]
        public Quiz Quiz { get; set; } = default!;

        public ICollection<QuizAnswer> QuizAnswers { get; set; } = new List<QuizAnswer>();
    }

    /// <summary>
    /// Loại câu hỏi trong Quiz.
    /// </summary>
    public enum QuestionType
    {
        MultipleChoice,
        TrueFalse,
        ShortAnswer
    }
}

