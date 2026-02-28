using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Enrollment;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implement
{
    public class EnrollmentService : IEnrollmentService
    {
        private readonly IEnrollmentRepository _enrollmentRepo;
        private readonly ICourseRepository _courseRepo;

        public EnrollmentService(IEnrollmentRepository enrollmentRepo, ICourseRepository courseRepo)
        {
            _enrollmentRepo = enrollmentRepo;
            _courseRepo = courseRepo;
        }

        public async Task<EnrollmentResult> EnrollAsync(string userId, Guid courseId)
        {
            // Kiểm tra khóa học tồn tại và đã xuất bản
            var course = await _courseRepo.GetByIdAsync(courseId);
            if (course == null || course.IsDeleted)
                return EnrollmentResult.Fail("Khóa học không tồn tại.");

            if (course.Status != CourseStatus.Published)
                return EnrollmentResult.Fail("Khóa học chưa được xuất bản.");

            // Kiểm tra đã ghi danh chưa
            if (await _enrollmentRepo.IsEnrolledAsync(userId, courseId))
                return EnrollmentResult.Fail("Bạn đã ghi danh khóa học này rồi.");

            // Khóa trả phí → cần kiểm tra đã thanh toán (OrderDetail)
            var effectivePrice = course.DiscountPrice ?? course.Price;
            if (effectivePrice > 0)
            {
                // Nếu khóa trả phí, chưa có payment flow → từ chối
                return EnrollmentResult.Fail("Khóa học này yêu cầu thanh toán. Vui lòng mua khóa học trước.");
            }

            // Khóa miễn phí → enroll ngay
            var enrollment = new Enrollment
            {
                EnrollmentId = Guid.NewGuid(),
                UserId = userId,
                CourseId = courseId,
                EnrolledAt = DateTime.UtcNow,
                IsActive = true,
                OrderDetailId = null
            };

            await _enrollmentRepo.CreateAsync(enrollment);
            return EnrollmentResult.Ok("Ghi danh thành công!");
        }

        public async Task<bool> IsEnrolledAsync(string userId, Guid courseId)
        {
            return await _enrollmentRepo.IsEnrolledAsync(userId, courseId);
        }

        public async Task<List<Enrollment>> GetMyEnrollmentsAsync(string userId)
        {
            return await _enrollmentRepo.GetByUserIdAsync(userId);
        }

        public async Task<Enrollment?> GetEnrollmentAsync(string userId, Guid courseId)
        {
            return await _enrollmentRepo.GetByUserAndCourseAsync(userId, courseId);
        }

        public async Task UpdateLastAccessedAsync(string userId, Guid courseId)
        {
            await _enrollmentRepo.UpdateLastAccessedAsync(userId, courseId);
        }
    }
}
