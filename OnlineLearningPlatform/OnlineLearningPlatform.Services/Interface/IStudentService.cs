using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IStudentService
    {
        Task<IEnumerable<Course>> GetAllCoursesWithEnrollmentStatusAsync(string userId);

        Task EnrollInCourseAsync(string userId, Guid courseId);

        Task<bool> IsEnrolledAsync(string userId, Guid courseId);
    }
}