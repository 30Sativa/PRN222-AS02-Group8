using CourseEntity = OnlineLearningPlatform.Models.Entities.Course;

namespace OnlineLearningPlatform.Services.DTOs.Course
{
    public class CourseCommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public CourseEntity? Course { get; set; }
    }
}
