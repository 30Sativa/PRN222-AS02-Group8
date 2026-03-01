namespace OnlineLearningPlatform.Services.DTOs.Dashboard
{
    /// <summary>
    /// DTO đại diện cho thông tin cơ bản của một khóa học dùng trên Dashboard.
    /// Chỉ lấy đúng những field cần hiển thị, không expose Entity ra ngoài Service layer.
    /// </summary>
    public class DashboardCourseDto
    {
        public Guid CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;

        /// <summary>Tên giảng viên tạo khóa học.</summary>
        public string TeacherName { get; set; } = string.Empty;

        /// <summary>Tên danh mục (null nếu chưa phân loại).</summary>
        public string? CategoryName { get; set; }

        public DateTime CreatedAt { get; set; }
    }

    /// <summary>
    /// DTO cho Top Courses: gộp thông tin khóa học + số lượt đăng ký.
    /// </summary>
    public class TopCourseDto
    {
        public DashboardCourseDto Course { get; set; } = new();

        /// <summary>Số học viên đang active trong khóa học.</summary>
        public int EnrollmentCount { get; set; }
    }
}
