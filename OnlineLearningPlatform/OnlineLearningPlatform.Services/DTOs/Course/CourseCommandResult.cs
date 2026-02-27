using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Services.DTOs.Course
{
    public class CourseCommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public Course? Course { get; set; }
    }
}
