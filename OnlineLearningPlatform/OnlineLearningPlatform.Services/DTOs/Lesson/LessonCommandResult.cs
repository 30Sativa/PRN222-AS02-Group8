using LessonEntity = OnlineLearningPlatform.Models.Entities.Lesson;

namespace OnlineLearningPlatform.Services.DTOs.Lesson
{
    public class LessonCommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public LessonEntity? Lesson { get; set; }
    }
}
