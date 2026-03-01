using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Một lần làm Quiz của học viên.
    /// </summary>
    public class QuizAttempt
    {
        [Key]
        public Guid AttemptId { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        public double? Score { get; set; }

        public bool IsPassed { get; set; } = false;

        public DateTime AttemptedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(QuizId))]
        public Quiz Quiz { get; set; } = default!;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;

        public ICollection<QuizAnswer> QuizAnswers { get; set; } = new List<QuizAnswer>();
    }
}

