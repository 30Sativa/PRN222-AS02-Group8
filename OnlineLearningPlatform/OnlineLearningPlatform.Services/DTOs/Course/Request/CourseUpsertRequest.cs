using OnlineLearningPlatform.Models.Entities;
using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.Services.DTOs.Course.Request
{
    public class CourseUpsertRequest
    {
        public Guid? CourseId { get; set; }

        [Required(ErrorMessage = "Mã khóa học là bắt buộc.")]
        [Display(Name = "Mã khóa học")]
        [MaxLength(20)]
        public string CourseCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tiêu đề khóa học là bắt buộc.")]
        [Display(Name = "Tiêu đề")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug là bắt buộc.")]
        [Display(Name = "Slug")]
        [MaxLength(220)]
        public string Slug { get; set; } = string.Empty;

        // Optional fields — no [Required]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Danh mục")]
        public int? CategoryId { get; set; }

        [Display(Name = "Giá")]
        [Range(0, 999999999, ErrorMessage = "Giá không hợp lệ.")]
        public decimal Price { get; set; }

        [Display(Name = "Giá khuyến mãi")]
        [Range(0, 999999999, ErrorMessage = "Giá khuyến mãi không hợp lệ.")]
        public decimal? DiscountPrice { get; set; }

        [Required(ErrorMessage = "Ngôn ngữ là bắt buộc.")]
        [Display(Name = "Ngôn ngữ")]
        [MaxLength(10)]
        public string Language { get; set; } = "vi";

        [Display(Name = "Trình độ")]
        public CourseLevel Level { get; set; } = CourseLevel.Beginner;

        [Display(Name = "Ảnh bìa (URL)")]
        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }
    }
}
