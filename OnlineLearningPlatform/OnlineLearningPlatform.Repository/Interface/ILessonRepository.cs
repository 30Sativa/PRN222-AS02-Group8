using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface ILessonRepository
    {
        Task<List<Lesson>> GetBySectionAsync(int sectionId);
        Task<Lesson?> GetByIdAsync(int lessonId);
        Task<Lesson> CreateAsync(Lesson lesson);
        Task<bool> UpdateAsync(Lesson lesson);
        Task<bool> DeleteAsync(int lessonId);
        Task<bool> IsLessonInTeacherCourseAsync(int lessonId, string teacherId);
        Task<bool> ExistsOrderIndexAsync(int sectionId, int orderIndex, int? excludeLessonId = null);
        Task<bool> ReorderAsync(int sectionId, List<int> orderedLessonIds);
    }
}
