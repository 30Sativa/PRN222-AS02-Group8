using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Student.Response;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IStudentService
    {
        Task<IEnumerable<Course>> GetAllCoursesWithEnrollmentStatusAsync(string userId);

        Task<(List<StudentCourseResponse> Courses, int TotalCount)> SearchCoursesAsync(string? keyword, string? userId, int pageNumber, int pageSize);
        Task<StudentCourseResponse?> GetCourseDetailAsync(Guid courseId, string? userId);

        Task EnrollInCourseAsync(string userId, Guid courseId);

        Task<bool> IsEnrolledAsync(string userId, Guid courseId);
    }
}