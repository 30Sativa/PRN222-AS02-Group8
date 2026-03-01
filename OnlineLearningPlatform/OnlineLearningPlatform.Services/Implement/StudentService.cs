using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Student.Response;
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

        public async Task<(List<StudentCourseResponse> Courses, int TotalCount)> SearchCoursesAsync(string? keyword, string? userId, int pageNumber, int pageSize)
        {
            var courses = await _courseRepo.SearchPublishedCoursesAsync(keyword);
            var mapped = new List<StudentCourseResponse>();

            foreach (var c in courses)
            {
                var isEnrolled = !string.IsNullOrEmpty(userId) && await _enrollmentRepo.IsEnrolledAsync(userId, c.CourseId);
                mapped.Add(new StudentCourseResponse
                {
                    CourseId = c.CourseId,
                    Title = c.Title,
                    Slug = c.Slug,
                    Description = c.Description,
                    Price = c.Price,
                    DiscountPrice = c.DiscountPrice,
                    ThumbnailUrl = c.ThumbnailUrl,
                    Level = c.Level.ToString(),
                    Language = c.Language,
                    CategoryName = c.Category?.CategoryName,
                    TeacherName = c.Teacher?.FullName,
                    IsEnrolled = isEnrolled
                });
            }

            var totalCount = mapped.Count;
            var paged = mapped.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();
            return (paged, totalCount);
        }

        public async Task<StudentCourseResponse?> GetCourseDetailAsync(Guid courseId, string? userId)
        {
            var c = await _courseRepo.GetByIdAsync(courseId);
            if (c == null || c.IsDeleted || c.Status != CourseStatus.Published) return null;

            var isEnrolled = !string.IsNullOrEmpty(userId) && await _enrollmentRepo.IsEnrolledAsync(userId, c.CourseId);
            return new StudentCourseResponse
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Slug = c.Slug,
                Description = c.Description,
                Price = c.Price,
                DiscountPrice = c.DiscountPrice,
                ThumbnailUrl = c.ThumbnailUrl,
                Level = c.Level.ToString(),
                Language = c.Language,
                CategoryName = c.Category?.CategoryName,
                TeacherName = c.Teacher?.FullName,
                IsEnrolled = isEnrolled
            };
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