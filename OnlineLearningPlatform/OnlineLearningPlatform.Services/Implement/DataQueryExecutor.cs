using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Chat;
using OnlineLearningPlatform.Services.Interface;

public class DataQueryExecutor : IDataQueryExecutor
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DataQueryExecutor> _logger;

    public DataQueryExecutor(ApplicationDbContext context, ILogger<DataQueryExecutor> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<QueryResult> ExecuteIntentAsync(QueryIntent intent, string userId)
    {
        try
        {
            switch (intent.Intent.ToLower())
            {
                case "list_courses":
                    return await ListCoursesAsync(intent.Parameters, intent.Limit, intent.OrderBy, intent.OrderDescending);
                case "count_courses":
                    return await CountCoursesAsync(intent.Parameters);
                case "list_enrollments":
                    return await ListEnrollmentsAsync(intent.Parameters, userId, intent.Limit);
                // Thêm các intent khác tương tự
                default:
                    return new QueryResult { ErrorMessage = $"Intent '{intent.Intent}' không được hỗ trợ." };
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi thực thi intent {Intent}", intent.Intent);
            return new QueryResult { ErrorMessage = "Đã xảy ra lỗi khi truy vấn dữ liệu." };
        }
    }

    private async Task<QueryResult> ListCoursesAsync(Dictionary<string, object> parameters, int? limit, string? orderBy, bool desc)
    {
        var query = _context.Courses
            .Where(c => c.Status == CourseStatus.Published && !c.IsDeleted)
            .AsQueryable();

        // Áp dụng bộ lọc từ parameters
        foreach (var param in parameters)
        {
            switch (param.Key.ToLower())
            {
                case "level":
                    if (Enum.TryParse<CourseLevel>(param.Value.ToString(), out var level))
                        query = query.Where(c => c.Level == level);
                    break;
                case "category":
                    query = query.Where(c => c.Category.CategoryName.Contains(param.Value.ToString()));
                    break;
                case "teacherid":
                    query = query.Where(c => c.TeacherId == param.Value.ToString());
                    break;
                case "minprice":
                    if (decimal.TryParse(param.Value.ToString(), out var min))
                        query = query.Where(c => c.Price >= min);
                    break;
                case "maxprice":
                    if (decimal.TryParse(param.Value.ToString(), out var max))
                        query = query.Where(c => c.Price <= max);
                    break;
            }
        }

        // Sắp xếp
        if (!string.IsNullOrEmpty(orderBy))
        {
            query = orderBy.ToLower() switch
            {
                "price" => desc ? query.OrderByDescending(c => c.Price) : query.OrderBy(c => c.Price),
                "createdat" => desc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                "title" => desc ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title),
                _ => query.OrderByDescending(c => c.CreatedAt) // mặc định
            };
        }
        else
        {
            query = query.OrderByDescending(c => c.CreatedAt);
        }

        // Giới hạn số lượng
        if (limit.HasValue)
            query = query.Take(limit.Value);

        var courses = await query
            .Select(c => new { c.Title, c.Price, c.Level, c.Category.CategoryName, c.Teacher.FullName })
            .ToListAsync();

        var result = new QueryResult();
        if (courses.Any())
        {
            result.Columns = new List<string> { "Title", "Price", "Level", "Category", "Teacher" };
            result.Rows = courses.Select(c => new List<string>
            {
                c.Title,
                c.Price.ToString("N0"),
                c.Level.ToString(),
                c.CategoryName,
                c.FullName
            }).ToList();
        }
        return result;
    }

    private async Task<QueryResult> CountCoursesAsync(Dictionary<string, object> parameters)
    {
        var query = _context.Courses
            .Where(c => c.Status == CourseStatus.Published && !c.IsDeleted)
            .AsQueryable();

        foreach (var param in parameters)
        {
            switch (param.Key.ToLower())
            {
                case "level":
                    if (Enum.TryParse<CourseLevel>(param.Value.ToString(), out var level))
                        query = query.Where(c => c.Level == level);
                    break;
                case "category":
                    query = query.Where(c => c.Category.CategoryName.Contains(param.Value.ToString()));
                    break;
                case "teacherid":
                    query = query.Where(c => c.TeacherId == param.Value.ToString());
                    break;
            }
        }

        int count = await query.CountAsync();
        var result = new QueryResult();
        result.Columns = new List<string> { "Count" };
        result.Rows = new List<List<string>> { new List<string> { count.ToString() } };
        return result;
    }

    private async Task<QueryResult> ListEnrollmentsAsync(Dictionary<string, object> parameters, string userId, int? limit)
    {
        // Mặc định lấy của user hiện tại nếu có tham số "my" hoặc không có userId khác
        bool myEnrollments = parameters.TryGetValue("my", out var val) && (val is bool b && b || val.ToString() == "true");
        string targetUserId = myEnrollments ? userId : parameters.GetValueOrDefault("userid")?.ToString();

        var query = _context.Enrollments
            .Where(e => e.IsActive)
            .Include(e => e.Course)
            .AsQueryable();

        if (!string.IsNullOrEmpty(targetUserId))
            query = query.Where(e => e.UserId == targetUserId);

        if (parameters.TryGetValue("courseid", out var courseIdObj) && Guid.TryParse(courseIdObj.ToString(), out var courseId))
            query = query.Where(e => e.CourseId == courseId);

        if (limit.HasValue)
            query = query.Take(limit.Value);

        var enrollments = await query
            .Select(e => new { e.Course.Title, e.EnrolledAt})
            .ToListAsync();

        var result = new QueryResult();
        if (enrollments.Any())
        {
            result.Columns = new List<string> { "Course", "EnrolledAt", "CompletedAt" };
            result.Rows = enrollments.Select(e => new List<string>
            {
                e.Title,
                e.EnrolledAt.ToString("yyyy-MM-dd"),
            }).ToList();
        }
        return result;
    }
}