using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.Dashboard;
using OnlineLearningPlatform.Services.Interface;
using System.Text.Json;

namespace OnlineLearningPlatform.RazorPages.Areas.Admin.Pages
{
    /// <summary>
    /// PageModel cho trang Admin Dashboard.
    /// Chỉ lấy dữ liệu từ Service layer, KHÔNG query thẳng Repository hay DbContext.
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class DashboardModel : PageModel
    {
        // Inject service tổng hợp dashboard thay vì inject trực tiếp UserService
        // → giúp PageModel gọn hơn, logic nghiệp vụ nằm ở Service
        private readonly IDashboardService _dashboardService;

        public DashboardModel(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        // ======================== PROPERTIES HIỂN THỊ TRÊN VIEW ========================

        // User stats (giữ lại để không phá compatible với Razor view cũ)
        public int TotalUsers { get; set; }
        public int TotalStudents { get; set; }
        public int TotalTeachers { get; set; }
        public int TotalAdmins { get; set; }

        // Course stats
        public int TotalCourses { get; set; }
        public int PendingCourses { get; set; }
        public int PublishedCourses { get; set; }

        // Enrollment stats
        public int TotalEnrollments { get; set; }
        public int NewEnrollmentsThisMonth { get; set; }

        // Revenue stats (định dạng tiền VND)
        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }

        // Pending actions (badge alert cho admin biết cần xử lý)
        public int PendingRefunds { get; set; }

        // Recent activities
        public DashboardStatsDto Stats { get; set; } = new();

        /// <summary>
        /// Dữ liệu chart doanh thu 12 tháng serialized thành JSON string.
        /// Razor Page truyền thẳng xuống JS để Chart.js vẽ biểu đồ.
        /// Không cần Ajax call thêm → giảm request.
        /// </summary>
        public string MonthlyRevenueJson { get; set; } = "[]";

        public async Task OnGetAsync()
        {
            // Gọi service một lần duy nhất để lấy tất cả stats (bên trong dùng Task.WhenAll song song)
            Stats = await _dashboardService.GetDashboardStatsAsync();

            // Map sang các property để view có thể dùng trực tiếp (backward compatible)
            TotalUsers              = Stats.TotalUsers;
            TotalStudents           = Stats.TotalStudents;
            TotalTeachers           = Stats.TotalTeachers;
            TotalAdmins             = Stats.TotalAdmins;

            TotalCourses            = Stats.TotalCourses;
            PendingCourses          = Stats.PendingCourses;
            PublishedCourses        = Stats.PublishedCourses;

            TotalEnrollments        = Stats.TotalEnrollments;
            NewEnrollmentsThisMonth = Stats.NewEnrollmentsThisMonth;

            TotalRevenue            = Stats.TotalRevenue;
            RevenueThisMonth        = Stats.RevenueThisMonth;

            PendingRefunds          = Stats.PendingRefunds;

            // Lấy dữ liệu chart và serialize thành JSON để JS đọc
            var monthlyRevenue = await _dashboardService.GetMonthlyRevenueAsync();

            // Serialize mảng decimal[] sang JSON, JS sẽ parse và vẽ Chart.js
            // Phải serialize như number không phải string để Chart.js không bị lỗi
            MonthlyRevenueJson = JsonSerializer.Serialize(monthlyRevenue);
        }
    }
}
