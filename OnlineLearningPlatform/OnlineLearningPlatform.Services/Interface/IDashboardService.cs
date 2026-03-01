using OnlineLearningPlatform.Services.DTOs.Dashboard;

namespace OnlineLearningPlatform.Services.Interface
{
    /// <summary>
    /// Service tổng hợp dữ liệu cho Admin Dashboard.
    /// Tất cả dữ liệu phải đi qua Service → Repository, KHÔNG query thẳng DbContext.
    /// </summary>
    public interface IDashboardService
    {
        /// <summary>
        /// Lấy tất cả thống kê cần thiết cho Admin Dashboard trong một lần gọi.
        /// Dùng WhenAll để chạy song song các query, tối ưu thời gian tải trang.
        /// </summary>
        Task<DashboardStatsDto> GetDashboardStatsAsync();

        /// <summary>
        /// Doanh thu theo từng tháng trong năm hiện tại.
        /// Trả về array 12 phần tử, index 0 = tháng 1.
        /// </summary>
        Task<decimal[]> GetMonthlyRevenueAsync();
    }
}
