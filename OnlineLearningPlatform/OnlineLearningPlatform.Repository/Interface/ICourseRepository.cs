using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface ICourseRepository
    {
        Task<List<Course>> GetByTeacherAsync(string teacherId);
        Task<Course?> GetByIdAndTeacherAsync(Guid courseId, string teacherId);
        Task<bool> ExistsByCodeAsync(string courseCode, Guid? excludeCourseId = null);
        Task<bool> ExistsBySlugAsync(string slug, Guid? excludeCourseId = null);
        Task<Course> CreateAsync(Course course);
        Task<bool> UpdateAsync(Course course);
        Task<bool> SoftDeleteAsync(Guid courseId, string teacherId);
    }
}
