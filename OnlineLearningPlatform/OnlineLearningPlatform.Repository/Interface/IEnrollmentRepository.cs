using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Repository.Interface
{
    public interface IEnrollmentRepository
    {
        /// <summary>
        /// Tạo enrollment mới.
        /// </summary>
        Task<Enrollment> CreateAsync(Enrollment enrollment);

        /// <summary>
        /// Kiểm tra học viên đã ghi danh khóa học chưa.
        /// </summary>
        Task<bool> IsEnrolledAsync(string userId, Guid courseId);

        /// <summary>
        /// Lấy enrollment theo userId và courseId.
        /// </summary>
        Task<Enrollment?> GetByUserAndCourseAsync(string userId, Guid courseId);

        /// <summary>
        /// Lấy tất cả enrollment (active) của một user, include Course + Sections + Lessons.
        /// </summary>
        Task<List<Enrollment>> GetByUserIdAsync(string userId);

        /// <summary>
        /// Lấy enrollment theo Id.
        /// </summary>
        Task<Enrollment?> GetByIdAsync(Guid enrollmentId);

        /// <summary>
        /// Cập nhật enrollment (vd: deactivate khi refund).
        /// </summary>
        Task<bool> UpdateAsync(Enrollment enrollment);

        /// <summary>
        /// Đếm số học viên đã ghi danh một khóa học.
        /// </summary>
        Task<int> CountByCourseAsync(Guid courseId);

        /// <summary>
        /// Cập nhật LastAccessedAt (dùng mỗi lần vào học).
        /// </summary>
        Task UpdateLastAccessedAsync(string userId, Guid courseId);
    }
}
