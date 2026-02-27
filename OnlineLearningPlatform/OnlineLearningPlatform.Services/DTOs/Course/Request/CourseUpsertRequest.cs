using OnlineLearningPlatform.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.Services.DTOs.Course.Request
{
    public class CourseUpsertRequest
    {
        public Guid? CourseId { get; set; }

        [Required]
        [MaxLength(20)]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(220)]
        public string Slug { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int? CategoryId { get; set; }

        [Range(0, 999999999)]
        public decimal Price { get; set; }

        [Range(0, 999999999)]
        public decimal? DiscountPrice { get; set; }

        [MaxLength(10)]
        public string Language { get; set; } = "vi";

        public CourseLevel Level { get; set; } = CourseLevel.Beginner;

        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }
    }
}
