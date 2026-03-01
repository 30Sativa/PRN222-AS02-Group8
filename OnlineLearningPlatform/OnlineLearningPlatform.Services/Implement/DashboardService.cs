using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;
using OnlineLearningPlatform.Services.DTOs.Dashboard;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.Services.Implement
{
    /// <summary>
    /// DashboardService: tổng hợp dữ liệu từ nhiều repository để phục vụ Admin Dashboard.
    ///
    /// ⚠️ QUAN TRỌNG — VÌ SAO KHÔNG DÙNG Task.WhenAll:
    /// EF Core DbContext KHÔNG PHẢI thread-safe.
    /// Một DbContext instance chỉ xử lý được 1 async operation tại một thời điểm.
    /// Nếu dùng Task.WhenAll, nhiều query sẽ chạy song song trên cùng 1 DbContext
    /// → lỗi "A second operation was started on this context instance".
    /// => PHẢI await tuần tự từng query.
    ///
    /// Tham khảo: https://go.microsoft.com/fwlink/?linkid=2097913
    /// </summary>
    public class DashboardService : IDashboardService
    {
        private readonly IDashboardRepository _dashboardRepo;
        private readonly IUserRepository _userRepo;

        public DashboardService(IDashboardRepository dashboardRepo, IUserRepository userRepo)
        {
            _dashboardRepo = dashboardRepo;
            _userRepo = userRepo;
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync()
        {
            // ===== AWAIT TUẦN TỰ — KHÔNG dùng Task.WhenAll với EF Core =====
            // DbContext không thread-safe → chỉ 1 query tại một thời điểm.
            // Mỗi await dưới đây chờ query hoàn thành rồi mới gọi cái tiếp theo.

            // --- User stats (qua UserManager / Identity) ---
            var totalUsers    = await _userRepo.CountUsersAsync();
            var students      = await _userRepo.GetUsersInRoleAsync("Student");
            var teachers      = await _userRepo.GetUsersInRoleAsync("Teacher");
            var admins        = await _userRepo.GetUsersInRoleAsync("Admin");

            // --- Course stats ---
            var totalCourses     = await _dashboardRepo.CountTotalCoursesAsync();
            var pendingCourses   = await _dashboardRepo.CountCoursesByStatusAsync(CourseStatus.Pending);
            var publishedCourses = await _dashboardRepo.CountCoursesByStatusAsync(CourseStatus.Published);

            // --- Enrollment stats ---
            var totalEnrollments    = await _dashboardRepo.CountTotalEnrollmentsAsync();
            var newEnrollments      = await _dashboardRepo.CountRecentEnrollmentsAsync(days: 30);

            // --- Revenue stats ---
            var totalRevenue       = await _dashboardRepo.GetTotalRevenueAsync();
            var revenueThisMonth   = await _dashboardRepo.GetRecentRevenueAsync(days: 30);

            // --- Pending actions ---
            var pendingRefunds = await _dashboardRepo.CountPendingRefundsAsync();

            // --- Recent activities ---
            var recentPendingCourses = await _dashboardRepo.GetRecentPendingCoursesAsync(limit: 5);
            var topCourses           = await _dashboardRepo.GetTopCoursesByEnrollmentAsync(limit: 5);

            // ===== Map Entity → DTO (tại Service layer) =====
            return new DashboardStatsDto
            {
                TotalUsers              = totalUsers,
                TotalStudents           = students.Count,
                TotalTeachers           = teachers.Count,
                TotalAdmins             = admins.Count,

                TotalCourses            = totalCourses,
                PendingCourses          = pendingCourses,
                PublishedCourses        = publishedCourses,

                TotalEnrollments        = totalEnrollments,
                NewEnrollmentsThisMonth = newEnrollments,

                TotalRevenue            = totalRevenue,
                RevenueThisMonth        = revenueThisMonth,

                PendingRefunds          = pendingRefunds,

                RecentPendingCourses    = MapToDashboardCourseDtos(recentPendingCourses),
                TopCourses              = MapToTopCourseDtos(topCourses)
            };
        }

        public async Task<decimal[]> GetMonthlyRevenueAsync()
        {
            // Lấy dict[month(1-12)] = revenue từ Repository
            var monthlyDict = await _dashboardRepo.GetMonthlyRevenueCurrentYearAsync();

            // Chuyển sang array 12 phần tử (index 0 = tháng 1)
            // RazorPage serialize mảng này thành JSON để Chart.js vẽ biểu đồ
            var result = new decimal[12];
            for (int month = 1; month <= 12; month++)
            {
                result[month - 1] = monthlyDict.TryGetValue(month, out var val) ? val : 0m;
            }
            return result;
        }

        // ======================== PRIVATE MAP HELPERS ========================

        /// <summary>
        /// Map list Course entity → list DashboardCourseDto.
        /// Teacher và Category đã được Include() từ Repository nên không cần query thêm.
        /// </summary>
        private static List<DashboardCourseDto> MapToDashboardCourseDtos(List<Course> courses)
        {
            return courses.Select(c => new DashboardCourseDto
            {
                CourseId     = c.CourseId,
                Title        = c.Title,
                CourseCode   = c.CourseCode,
                TeacherName  = c.Teacher?.FullName ?? string.Empty,
                CategoryName = c.Category?.CategoryName,
                CreatedAt    = c.CreatedAt
            }).ToList();
        }

        /// <summary>
        /// Map list (Course, int) tuple → list TopCourseDto.
        /// </summary>
        private static List<TopCourseDto> MapToTopCourseDtos(List<(Course Course, int EnrollmentCount)> items)
        {
            return items.Select(item => new TopCourseDto
            {
                Course = new DashboardCourseDto
                {
                    CourseId     = item.Course.CourseId,
                    Title        = item.Course.Title,
                    CourseCode   = item.Course.CourseCode,
                    TeacherName  = item.Course.Teacher?.FullName ?? string.Empty,
                    CategoryName = item.Course.Category?.CategoryName,
                    CreatedAt    = item.Course.CreatedAt
                },
                EnrollmentCount = item.EnrollmentCount
            }).ToList();
        }
    }
}
