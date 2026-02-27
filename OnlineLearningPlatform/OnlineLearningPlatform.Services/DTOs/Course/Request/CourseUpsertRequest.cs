using OnlineLearningPlatform.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.Services.DTOs.Course.Request
{
    public class CourseUpsertRequest
    {
        public Guid? CourseId { get; set; }

        [Required]
        [Display(Name = "Course Code")]
        [MaxLength(20)]
        public string CourseCode { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Title")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Slug")]
        [MaxLength(220)]
        public string Slug { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        [Required]
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        [Required]
        [Display(Name = "Price")]
        [Range(0, 999999999)]
        public decimal Price { get; set; }

        [Display(Name = "Discount Price")]
        [Range(0, 999999999)]
        public decimal? DiscountPrice { get; set; }

        [Required]
        [Display(Name = "Language")]
        [MaxLength(10)]
        public string Language { get; set; } = "vi";

        [Required]
        [Display(Name = "Level")]
        public CourseLevel Level { get; set; } = CourseLevel.Beginner;

        [Required]
        [Display(Name = "Thumbnail URL")]
        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }
    }
}
