using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Lesson;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface ILessonService
    {
        Task<List<Lesson>> GetBySectionAsync(int sectionId);
        Task<Lesson?> GetByIdAsync(int lessonId);
        Task<LessonCommandResult> CreateAsync(Lesson lesson, string teacherId);
        Task<LessonCommandResult> UpdateAsync(Lesson lesson, string teacherId);
        Task<LessonCommandResult> DeleteAsync(int lessonId, string teacherId);
    }
}
