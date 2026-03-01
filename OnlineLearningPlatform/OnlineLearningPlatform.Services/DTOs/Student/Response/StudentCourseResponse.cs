using System;

namespace OnlineLearningPlatform.Services.DTOs.Student.Response
{
    public class StudentCourseResponse
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? DiscountPrice { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string Level { get; set; } = null!;
        public string Language { get; set; } = null!;
        
        public string? CategoryName { get; set; }
        public string? TeacherName { get; set; }
        
        // Trạng thái khóa học đối với Student hiện tại
        public bool IsEnrolled { get; set; }
    }
}
