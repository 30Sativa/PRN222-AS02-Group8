using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Course;
using OnlineLearningPlatform.Services.DTOs.Course.Request;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface ICourseService
    {
        Task<List<Course>> GetMyCoursesAsync(string teacherId);
        Task<Course?> GetMyCourseByIdAsync(Guid courseId, string teacherId);

        Task<List<Course>> GetAllForAdminAsync();
        Task<Course?> GetByIdForAdminAsync(Guid courseId);

        Task<CourseCommandResult> CreateAsync(string teacherId, CourseUpsertRequest request);
        Task<CourseCommandResult> UpdateAsync(string teacherId, CourseUpsertRequest request);
        Task<CourseCommandResult> DeleteAsync(Guid courseId, string teacherId);

        Task<CourseCommandResult> ApproveAsync(Guid courseId);
        Task<CourseCommandResult> RejectAsync(Guid courseId, string reason);
    }
}
