using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Repository.Interface;

namespace OnlineLearningPlatform.Repository.Implement
{
    /// <summary>
    /// Implementation của IDashboardRepository.
    /// Dùng ApplicationDbContext để query dữ liệu tổng hợp cho Admin Dashboard.
    ///
    /// ARCHITECTURE:
    /// - Repository chỉ trả Entity, KHÔNG trả DTO (tránh circular dependency với Services).
    /// - Tất cả query dùng AsNoTracking() vì chỉ đọc dữ liệu, không cần tracking → tăng hiệu năng.
    /// - Mapping Entity → DTO là trách nhiệm của DashboardService.
    /// </summary>
    public class DashboardRepository : IDashboardRepository
    {
        private readonly ApplicationDbContext _context;

        public DashboardRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // ======================== COURSES ========================

        public async Task<int> CountTotalCoursesAsync()
        {
            // Chỉ đếm khóa học chưa bị soft-delete
            return await _context.Courses
                .AsNoTracking()
                .CountAsync(c => !c.IsDeleted);
        }

        public async Task<int> CountCoursesByStatusAsync(CourseStatus status)
        {
            return await _context.Courses
                .AsNoTracking()
                .CountAsync(c => !c.IsDeleted && c.Status == status);
        }

        // ======================== ENROLLMENTS ========================

        public async Task<int> CountTotalEnrollmentsAsync()
        {
            // Chỉ đếm enrollment đang active (IsActive = true).
            // Enrollment bị deactivate khi refund → không tính vào số liệu.
            return await _context.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.IsActive);
        }

        public async Task<int> CountRecentEnrollmentsAsync(int days)
        {
            // Tính mốc thời gian bắt đầu của khoảng thống kê
            var from = DateTime.UtcNow.AddDays(-days);
            return await _context.Enrollments
                .AsNoTracking()
                .CountAsync(e => e.IsActive && e.EnrolledAt >= from);
        }

        // ======================== REVENUE ========================

        public async Task<decimal> GetTotalRevenueAsync()
        {
            // Chỉ tính đơn hàng COMPLETED (đã thanh toán thành công).
            // Không tính Pending (chưa xong), Failed (thất bại), Refunded (đã hoàn).
            // Dùng (decimal?) để tránh lỗi khi không có record nào → SumAsync trả null → coalesce 0.
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Completed)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
        }

        public async Task<decimal> GetRecentRevenueAsync(int days)
        {
            var from = DateTime.UtcNow.AddDays(-days);
            return await _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Completed && o.CompletedAt >= from)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;
        }

        // ======================== PENDING ACTIONS ========================

        public async Task<int> CountPendingRefundsAsync()
        {
            // RefundStatus.Pending = chờ admin duyệt hoặc từ chối
            return await _context.Refunds
                .AsNoTracking()
                .CountAsync(r => r.Status == RefundStatus.Pending);
        }

        // ======================== RECENT ACTIVITIES ========================

        public async Task<List<Course>> GetRecentPendingCoursesAsync(int limit)
        {
            // Lấy N khóa học mới nhất đang Pending, mới nhất trước.
            // Include Teacher & Category để Service map sang DTO mà không cần query thêm.
            return await _context.Courses
                .AsNoTracking()
                .Include(c => c.Teacher)
                .Include(c => c.Category)
                .Where(c => !c.IsDeleted && c.Status == CourseStatus.Pending)
                .OrderByDescending(c => c.CreatedAt)
                .Take(limit)
                .ToListAsync();
        }

        public async Task<List<(Course Course, int EnrollmentCount)>> GetTopCoursesByEnrollmentAsync(int limit)
        {
            // Select ra anonymous type trước, rồi chuyển sang ValueTuple.
            // Không thể dùng ValueTuple trực tiếp trong LINQ-to-SQL nên cần bước trung gian này.
            var rows = await _context.Courses
                .AsNoTracking()
                .Include(c => c.Teacher)
                .Include(c => c.Category)
                .Where(c => !c.IsDeleted && c.Status == CourseStatus.Published)
                .Select(c => new
                {
                    Course = c,
                    // EF Core dịch Count() thành subquery COUNT(*) — hiệu quả hơn load cả danh sách enrollment
                    EnrollmentCount = c.Enrollments.Count(e => e.IsActive)
                })
                .OrderByDescending(x => x.EnrollmentCount)
                .Take(limit)
                .ToListAsync();

            // Chuyển anonymous type → ValueTuple (tương thích với interface)
            return rows.Select(r => (r.Course, r.EnrollmentCount)).ToList();
        }

        public async Task<Dictionary<int, decimal>> GetMonthlyRevenueCurrentYearAsync()
        {
            var currentYear = DateTime.UtcNow.Year;

            // GroupBy tháng trong năm hiện tại, tính tổng doanh thu mỗi tháng
            var monthlyData = await _context.Orders
                .AsNoTracking()
                .Where(o => o.Status == OrderStatus.Completed
                         && o.CompletedAt.HasValue
                         && o.CompletedAt.Value.Year == currentYear)
                .GroupBy(o => o.CompletedAt!.Value.Month)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(o => o.TotalAmount) })
                .ToListAsync();

            // Khởi tạo dict 12 tháng với giá trị 0 để chart hiển thị đủ 12 cột
            var result = Enumerable.Range(1, 12).ToDictionary(m => m, _ => 0m);

            // Fill dữ liệu thực vào dict
            foreach (var item in monthlyData)
                result[item.Month] = item.Revenue;

            return result;
        }
    }
}
