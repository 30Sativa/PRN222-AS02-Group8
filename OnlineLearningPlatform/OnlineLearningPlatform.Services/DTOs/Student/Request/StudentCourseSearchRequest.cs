using System.ComponentModel.DataAnnotations;

namespace OnlineLearningPlatform.Services.DTOs.Student.Request
{
    public class StudentCourseSearchRequest
    {
        public string? Keyword { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
    }
}
