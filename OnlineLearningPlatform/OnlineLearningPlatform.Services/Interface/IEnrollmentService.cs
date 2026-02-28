using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Enrollment;

namespace OnlineLearningPlatform.Services.Interface
{
    public interface IEnrollmentService
    {
        /// <summary>
        /// Ghi danh học viên vào khóa học.
        /// Kiểm tra: khóa free → enroll ngay, khóa trả phí → cần đã thanh toán.
        /// </summary>
        Task<EnrollmentResult> EnrollAsync(string userId, Guid courseId);

        /// <summary>
        /// Kiểm tra học viên đã ghi danh (active) chưa.
        /// </summary>
        Task<bool> IsEnrolledAsync(string userId, Guid courseId);

        /// <summary>
        /// Lấy danh sách enrollment của user (kèm thông tin Course).
        /// </summary>
        Task<List<Enrollment>> GetMyEnrollmentsAsync(string userId);

        /// <summary>
        /// Lấy enrollment cụ thể (dùng cho trang Learn).
        /// </summary>
        Task<Enrollment?> GetEnrollmentAsync(string userId, Guid courseId);

        /// <summary>
        /// Cập nhật LastAccessedAt mỗi khi học viên xem bài.
        /// </summary>
        Task UpdateLastAccessedAsync(string userId, Guid courseId);
    }
}
