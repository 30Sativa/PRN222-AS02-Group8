using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implement
{
    public class StudentService : IStudentService
    {
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly ICourseRepository _courseRepo; 

        public StudentService(IEnrollmentRepository enrollmentRepo, ICourseRepository courseRepo)
        {
            _enrollmentRepo = enrollmentRepo;
            _courseRepo = courseRepo;
        }

        public async Task<IEnumerable<Course>> GetAllCoursesWithEnrollmentStatusAsync(string userId)
        {
            return await _courseRepo.GetPublishedCoursesWithEnrollmentsAsync();
        }

        public async Task EnrollInCourseAsync(string userId, Guid courseId)
        {
            var alreadyEnrolled = await IsEnrolledAsync(userId, courseId);
            if (alreadyEnrolled) return;

            var enrollment = new Enrollment
            {
                EnrollmentId = Guid.NewGuid(),
                CourseId = courseId,
                UserId = userId,
                EnrolledAt = DateTime.UtcNow,
                IsActive = true
            };

            await _enrollmentRepo.CreateAsync(enrollment);
        }

        public async Task<bool> IsEnrolledAsync(string userId, Guid courseId)
        {
            return await _enrollmentRepo.IsEnrolledAsync(userId, courseId);
        }
    }
}