namespace OnlineLearningPlatform.Services.DTOs.Dashboard
{
    /// <summary>
    /// DTO chứa tất cả số liệu thống kê cho Admin Dashboard.
    /// Đóng gói thành một object để PageModel chỉ cần inject 1 service và gọi 1 method.
    ///
    /// RULE: DTO layer không được import Entity trực tiếp.
    /// Dùng DashboardCourseDto / TopCourseDto thay cho Course entity.
    /// </summary>
    public class DashboardStatsDto
    {
        // ======================== USER STATS ========================

        /// <summary>Tổng người dùng hệ thống.</summary>
        public int TotalUsers { get; set; }

        /// <summary>Số học viên (role Student).</summary>
        public int TotalStudents { get; set; }

        /// <summary>Số giảng viên (role Teacher).</summary>
        public int TotalTeachers { get; set; }

        /// <summary>Số quản trị viên (role Admin).</summary>
        public int TotalAdmins { get; set; }

        // ======================== COURSE STATS ========================

        /// <summary>Tổng khóa học trong hệ thống (chưa xóa).</summary>
        public int TotalCourses { get; set; }

        /// <summary>Số khóa học đang chờ duyệt.</summary>
        public int PendingCourses { get; set; }

        /// <summary>Số khóa học đã xuất bản.</summary>
        public int PublishedCourses { get; set; }

        // ======================== ENROLLMENT STATS ========================

        /// <summary>Tổng lượt đăng ký (active).</summary>
        public int TotalEnrollments { get; set; }

        /// <summary>Số lượt đăng ký mới trong 30 ngày gần đây.</summary>
        public int NewEnrollmentsThisMonth { get; set; }

        // ======================== REVENUE STATS ========================

        /// <summary>Tổng doanh thu từ đơn hàng hoàn thành.</summary>
        public decimal TotalRevenue { get; set; }

        /// <summary>Doanh thu trong 30 ngày gần đây.</summary>
        public decimal RevenueThisMonth { get; set; }

        // ======================== PENDING ACTIONS (cần admin xử lý) ========================

        /// <summary>Số yêu cầu hoàn tiền đang chờ duyệt.</summary>
        public int PendingRefunds { get; set; }

        // ======================== RECENT ACTIVITIES ========================

        /// <summary>Danh sách N khóa học mới nhất đang chờ duyệt (dùng DashboardCourseDto, không dùng Entity).</summary>
        public List<DashboardCourseDto> RecentPendingCourses { get; set; } = new();

        /// <summary>Top N khóa học có nhiều lượt đăng ký nhất (dùng TopCourseDto, không dùng Entity).</summary>
        public List<TopCourseDto> TopCourses { get; set; } = new();
    }
}
