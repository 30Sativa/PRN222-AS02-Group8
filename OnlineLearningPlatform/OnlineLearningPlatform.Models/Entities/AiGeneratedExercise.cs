using OnlineLearningPlatform.Models.Entities.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace OnlineLearningPlatform.Models.Entities
{
    /// <summary>
    /// Bài tập được AI sinh ra cho user (có thể gắn với một Course).
    /// </summary>
    public class AiGeneratedExercise
    {
        [Key]
        public Guid ExerciseId { get; set; }

        [Required]
        public string UserId { get; set; } = default!;

        public Guid? CourseId { get; set; }

        [Required]
        public string Content { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; } = default!;
    }
}

