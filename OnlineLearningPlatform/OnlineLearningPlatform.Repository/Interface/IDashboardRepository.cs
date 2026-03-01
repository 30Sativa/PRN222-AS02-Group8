using OnlineLearningPlatform.Models.Entities;

namespace OnlineLearningPlatform.Repository.Interface
{
    /// <summary>
    /// Repository cung cấp dữ liệu tổng hợp cho Admin Dashboard.
    ///
    /// ARCHITECTURE NOTE:
    /// Repository trả về Entity (Course) — không trả DTO.
    /// Việc map Entity → DTO là trách nhiệm của Service layer.
    /// Lý do: Repository không được phép biết về DTO của Service (tránh circular dependency).
    ///
    /// Dependency chain đúng: RazorPages → Services → Repository → Models
    /// </summary>
    public interface IDashboardRepository
    {
        // ======================== COURSES ========================

        /// <summary>Tổng số khóa học chưa bị xóa.</summary>
        Task<int> CountTotalCoursesAsync();

        /// <summary>Số khóa học theo từng trạng thái (Pending / Published / Rejected).</summary>
        Task<int> CountCoursesByStatusAsync(CourseStatus status);

        // ======================== ENROLLMENTS ========================

        /// <summary>Tổng số lượt đăng ký đang active.</summary>
        Task<int> CountTotalEnrollmentsAsync();

        /// <summary>
        /// Số lượt đăng ký mới trong N ngày gần đây (dùng cho thống kê tăng trưởng).
        /// </summary>
        Task<int> CountRecentEnrollmentsAsync(int days);

        // ======================== REVENUE ========================

        /// <summary>
        /// Tổng doanh thu từ các Order đã hoàn thành (OrderStatus.Completed).
        /// Đây là doanh thu thực thu, đã loại bỏ đơn Pending / Failed / Refunded.
        /// </summary>
        Task<decimal> GetTotalRevenueAsync();

        /// <summary>Doanh thu trong N ngày gần đây.</summary>
        Task<decimal> GetRecentRevenueAsync(int days);

        // ======================== PENDING ACTIONS ========================

        /// <summary>Số yêu cầu hoàn tiền đang chờ xử lý (cần admin phê duyệt).</summary>
        Task<int> CountPendingRefundsAsync();

        // ======================== RECENT ACTIVITIES ========================

        /// <summary>
        /// Lấy danh sách khóa học mới nhất đang chờ duyệt (Pending), giới hạn N bản ghi.
        /// Include Teacher và Category để Service map được sang DTO.
        /// </summary>
        Task<List<Course>> GetRecentPendingCoursesAsync(int limit);

        /// <summary>
        /// Lấy top N khóa học có nhiều học viên đăng ký nhất (Published, chưa bị xóa).
        /// Trả về tuple Entity + count để Service tự map sang TopCourseDto.
        /// NOTE: Dùng ValueTuple thay anonymous type để interface khai báo được.
        /// </summary>
        Task<List<(Course Course, int EnrollmentCount)>> GetTopCoursesByEnrollmentAsync(int limit);

        /// <summary>
        /// Doanh thu theo từng tháng trong năm hiện tại (12 tháng).
        /// Trả về Dictionary[month(1-12)] = revenue.
        /// </summary>
        Task<Dictionary<int, decimal>> GetMonthlyRevenueCurrentYearAsync();
    }
}
