using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Chat;
using OnlineLearningPlatform.Services.Interface;
using System.Text.RegularExpressions;

namespace OnlineLearningPlatform.Services.Implement
{
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
                return intent.Intent.ToLower() switch
                {
                    // ==================== COURSE INTENTS ====================
                    "list_courses" => await ListCoursesAsync(intent.Parameters, intent.Limit, intent.OrderBy, intent.OrderDescending),
                    "count_courses" => await CountCoursesAsync(intent.Parameters),
                    "get_course_detail" => await GetCourseDetailAsync(intent.Parameters),
                    "get_popular_courses" => await GetPopularCoursesAsync(intent.Limit),
                    "get_top_rated_courses" => await GetTopRatedCoursesAsync(intent.Limit),
                    "get_featured_courses" => await GetFeaturedCoursesAsync(intent.Limit),

                    // ==================== ENROLLMENT INTENTS ====================
                    "list_enrollments" => await ListEnrollmentsAsync(intent.Parameters, userId, intent.Limit),
                    "count_enrollments" => await CountEnrollmentsAsync(intent.Parameters, userId),
                    "get_user_stats" => await GetUserStatsAsync(intent.Parameters, userId),
                    "get_user_progress" => await GetUserProgressAsync(intent.Parameters, userId),

                    // ==================== REVIEW INTENTS ====================
                    "list_reviews" => await ListReviewsAsync(intent.Parameters, intent.Limit),

                    // ==================== WALLET INTENTS ====================
                    "get_wallet_balance" => await GetWalletBalanceAsync(intent.Parameters, userId),
                    "get_wallet_transactions" => await GetWalletTransactionsAsync(intent.Parameters, userId, intent.Limit),

                    // ==================== CATEGORY INTENTS ====================
                    "list_categories" => await ListCategoriesAsync(intent.Limit),

                    // ==================== CERTIFICATE INTENTS ====================
                    "get_user_certificates" => await GetUserCertificatesAsync(intent.Parameters, userId, intent.Limit),

                    // ==================== QUIZ INTENTS ====================
                    "get_user_quiz_stats" => await GetUserQuizStatsAsync(intent.Parameters, userId),

                    // ==================== NOTIFICATION INTENTS ====================
                    "get_user_notifications" => await GetUserNotificationsAsync(intent.Parameters, userId, intent.Limit),

                    _ => new QueryResult { ErrorMessage = $"Intent '{intent.Intent}' không được hỗ trợ." }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thực thi intent {Intent}", intent.Intent);
                return new QueryResult { ErrorMessage = "Đã xảy ra lỗi khi truy vấn dữ liệu: " + ex.Message };
            }
        }

        // ==================== COURSE METHODS ====================

        private async Task<QueryResult> ListCoursesAsync(Dictionary<string, object> parameters, int? limit, string? orderBy, bool desc)
        {
            var query = _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                .Include(c => c.Reviews)
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
                        query = query.Where(c => c.Category != null && c.Category.CategoryName.Contains(param.Value.ToString()));
                        break;
                    case "teacherid":
                        query = query.Where(c => c.TeacherId == param.Value.ToString());
                        break;
                    case "minprice":
                        if (decimal.TryParse(param.Value.ToString(), out var min))
                            query = query.Where(c => (c.DiscountPrice ?? c.Price) >= min);
                        break;
                    case "maxprice":
                        if (decimal.TryParse(param.Value.ToString(), out var max))
                            query = query.Where(c => (c.DiscountPrice ?? c.Price) <= max);
                        break;
                    case "free":
                        if (bool.TryParse(param.Value.ToString(), out var isFree) && isFree)
                            query = query.Where(c => c.Price == 0);
                        break;
                    case "search":
                        var search = param.Value.ToString() ?? "";
                        query = query.Where(c => c.Title.Contains(search) || (c.Description != null && c.Description.Contains(search)));
                        break;
                }
            }

            if (!string.IsNullOrEmpty(orderBy))
            {
                query = orderBy.ToLower() switch
                {
                    "price" => desc ? query.OrderByDescending(c => c.Price) : query.OrderBy(c => c.Price),
                    "createdat" => desc ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                    "title" => desc ? query.OrderByDescending(c => c.Title) : query.OrderBy(c => c.Title),
                    "rating" => query.OrderByDescending(c => c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0),
                    _ => query.OrderByDescending(c => c.CreatedAt)
                };
            }
            else
            {
                query = query.OrderByDescending(c => c.CreatedAt);
            }

            if (limit.HasValue)
                query = query.Take(limit.Value);

            var courses = await query
                .Select(c => new
                {
                    c.Title,
                    Price = c.DiscountPrice ?? c.Price,
                    c.Level,
                    Category = c.Category != null ? c.Category.CategoryName : "N/A",
                    Teacher = c.Teacher.FullName,
                    EnrollmentCount = c.Enrollments.Count,
                    AvgRating = c.Reviews.Any() ? c.Reviews.Average(r => r.Rating) : 0
                })
                .ToListAsync();

            var result = new QueryResult();
            if (courses.Any())
            {
                result.Columns = new List<string> { "Title", "Price", "Level", "Category", "Teacher", "Students", "Rating" };
                result.Rows = courses.Select(c => new List<string>
                {
                    c.Title,
                    c.Price.ToString("N0") + " VNĐ",
                    c.Level.ToString(),
                    c.Category,
                    c.Teacher,
                    c.EnrollmentCount.ToString(),
                    c.AvgRating.ToString("F1") + "/5"
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
                        query = query.Where(c => c.Category != null && c.Category.CategoryName.Contains(param.Value.ToString()));
                        break;
                    case "teacherid":
                        query = query.Where(c => c.TeacherId == param.Value.ToString());
                        break;
                }
            }

            int count = await query.CountAsync();
            var result = new QueryResult();
            result.Columns = new List<string> { "TotalCourses" };
            result.Rows = new List<List<string>> { new List<string> { count.ToString() } };
            return result;
        }

        private async Task<QueryResult> GetCourseDetailAsync(Dictionary<string, object> parameters)
        {
            Course? course = null;

            if (parameters.TryGetValue("courseid", out var courseIdObj) && Guid.TryParse(courseIdObj.ToString(), out var courseId))
            {
                course = await _context.Courses
                    .Include(c => c.Category)
                    .Include(c => c.Teacher)
                    .Include(c => c.Sections).ThenInclude(s => s.Lessons)
                    .Include(c => c.Reviews).ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(c => c.CourseId == courseId);
            }
            else if (parameters.TryGetValue("title", out var titleObj))
            {
                var title = titleObj.ToString() ?? "";
                course = await _context.Courses
                    .Include(c => c.Category)
                    .Include(c => c.Teacher)
                    .Include(c => c.Sections).ThenInclude(s => s.Lessons)
                    .Include(c => c.Reviews).ThenInclude(r => r.User)
                    .FirstOrDefaultAsync(c => c.Title.Contains(title) && c.Status == CourseStatus.Published);
            }

            var result = new QueryResult();
            if (course != null)
            {
                var sectionCount = course.Sections.Count;
                var lessonCount = course.Sections.SelectMany(s => s.Lessons).Count();
                var totalDuration = course.TotalDuration;
                var avgRating = course.Reviews.Any() ? course.Reviews.Average(r => r.Rating) : 0;

                result.Columns = new List<string> { "Info", "Value" };
                result.Rows = new List<List<string>>
                {
                    new() { "Title", course.Title },
                    new() { "Code", course.CourseCode },
                    new() { "Price", (course.DiscountPrice ?? course.Price).ToString("N0") + " VNĐ" },
                    new() { "Level", course.Level.ToString() },
                    new() { "Category", course.Category?.CategoryName ?? "N/A" },
                    new() { "Teacher", course.Teacher.FullName },
                    new() { "Sections", sectionCount.ToString() },
                    new() { "Lessons", lessonCount.ToString() },
                    new() { "Duration", TimeSpan.FromSeconds(course.TotalDuration).ToString(@"hh\:mm\:ss") },
                    new() { "Students", course.Enrollments.Count.ToString() },
                    new() { "Reviews", course.Reviews.Count.ToString() },
                    new() { "Avg Rating", avgRating.ToString("F1") + "/5" },
                    new() { "Created", course.CreatedAt.ToString("dd/MM/yyyy") }
                };
            }
            return result;
        }

        private async Task<QueryResult> GetPopularCoursesAsync(int? limit)
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                .Where(c => c.Status == CourseStatus.Published && !c.IsDeleted)
                .OrderByDescending(c => c.Enrollments.Count)
                .Take(limit ?? 10)
                .Select(c => new
                {
                    c.Title,
                    Price = c.DiscountPrice ?? c.Price,
                    c.Level,
                    Category = c.Category != null ? c.Category.CategoryName : "N/A",
                    Teacher = c.Teacher.FullName,
                    EnrollmentCount = c.Enrollments.Count
                })
                .ToListAsync();

            var result = new QueryResult();
            if (courses.Any())
            {
                result.Columns = new List<string> { "Title", "Price", "Level", "Category", "Teacher", "Students" };
                result.Rows = courses.Select(c => new List<string>
                {
                    c.Title,
                    c.Price.ToString("N0") + " VNĐ",
                    c.Level.ToString(),
                    c.Category,
                    c.Teacher,
                    c.EnrollmentCount.ToString()
                }).ToList();
            }
            return result;
        }

        private async Task<QueryResult> GetTopRatedCoursesAsync(int? limit)
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                .Include(c => c.Reviews)
                .Where(c => c.Status == CourseStatus.Published && !c.IsDeleted && c.Reviews.Any())
                .OrderByDescending(c => c.Reviews.Average(r => r.Rating))
                .Take(limit ?? 10)
                .Select(c => new
                {
                    c.Title,
                    Price = c.DiscountPrice ?? c.Price,
                    c.Level,
                    Category = c.Category != null ? c.Category.CategoryName : "N/A",
                    Teacher = c.Teacher.FullName,
                    AvgRating = c.Reviews.Average(r => r.Rating),
                    ReviewCount = c.Reviews.Count
                })
                .ToListAsync();

            var result = new QueryResult();
            if (courses.Any())
            {
                result.Columns = new List<string> { "Title", "Price", "Level", "Category", "Teacher", "Rating", "Reviews" };
                result.Rows = courses.Select(c => new List<string>
                {
                    c.Title,
                    c.Price.ToString("N0") + " VNĐ",
                    c.Level.ToString(),
                    c.Category,
                    c.Teacher,
                    c.AvgRating.ToString("F1") + "/5",
                    c.ReviewCount.ToString()
                }).ToList();
            }
            return result;
        }

        private async Task<QueryResult> GetFeaturedCoursesAsync(int? limit)
        {
            var courses = await _context.Courses
                .Include(c => c.Category)
                .Include(c => c.Teacher)
                .Where(c => c.Status == CourseStatus.Published && !c.IsDeleted && c.IsFeatured)
                .Take(limit ?? 10)
                .Select(c => new
                {
                    c.Title,
                    Price = c.DiscountPrice ?? c.Price,
                    c.Level,
                    Category = c.Category != null ? c.Category.CategoryName : "N/A",
                    Teacher = c.Teacher.FullName,
                    EnrollmentCount = c.Enrollments.Count
                })
                .ToListAsync();

            var result = new QueryResult();
            if (courses.Any())
            {
                result.Columns = new List<string> { "Title", "Price", "Level", "Category", "Teacher", "Students" };
                result.Rows = courses.Select(c => new List<string>
                {
                    c.Title,
                    c.Price.ToString("N0") + " VNĐ",
                    c.Level.ToString(),
                    c.Category,
                    c.Teacher,
                    c.EnrollmentCount.ToString()
                }).ToList();
            }
            return result;
        }

        // ==================== ENROLLMENT METHODS ====================

        private async Task<QueryResult> ListEnrollmentsAsync(Dictionary<string, object> parameters, string userId, int? limit)
        {
            bool myEnrollments = parameters.TryGetValue("my", out var val) && (val is bool b && b || val.ToString() == "true");
            string? targetUserId = myEnrollments ? userId : parameters.GetValueOrDefault("userid")?.ToString();

            var query = _context.Enrollments
                .Where(e => e.IsActive)
                .Include(e => e.Course).ThenInclude(c => c.Category)
                .Include(e => e.Course).ThenInclude(c => c.Teacher)
                .AsQueryable();

            if (!string.IsNullOrEmpty(targetUserId))
                query = query.Where(e => e.UserId == targetUserId);

            if (parameters.TryGetValue("courseid", out var courseIdObj) && Guid.TryParse(courseIdObj.ToString(), out var courseId))
                query = query.Where(e => e.CourseId == courseId);

            if (limit.HasValue)
                query = query.Take(limit.Value);

            query = query.OrderByDescending(e => e.EnrolledAt);

            var enrollments = await query.ToListAsync();

            var result = new QueryResult();
            if (enrollments.Any())
            {
                result.Columns = new List<string> { "Course", "Category", "Teacher", "EnrolledAt", "LastAccessed" };
                result.Rows = enrollments.Select(e => new List<string>
                {
                    e.Course.Title,
                    e.Course.Category?.CategoryName ?? "N/A",
                    e.Course.Teacher.FullName,
                    e.EnrolledAt.ToString("dd/MM/yyyy"),
                    e.LastAccessedAt?.ToString("dd/MM/yyyy") ?? "Chưa truy cập"
                }).ToList();
            }
            return result;
        }

        private async Task<QueryResult> CountEnrollmentsAsync(Dictionary<string, object> parameters, string userId)
        {
            bool myEnrollments = parameters.TryGetValue("my", out var val) && (val is bool b && b || val.ToString() == "true");
            string? targetUserId = myEnrollments ? userId : parameters.GetValueOrDefault("userid")?.ToString();

            var query = _context.Enrollments.Where(e => e.IsActive);

            if (!string.IsNullOrEmpty(targetUserId))
                query = query.Where(e => e.UserId == targetUserId);

            int count = await query.CountAsync();
            var result = new QueryResult();
            result.Columns = new List<string> { "TotalEnrollments" };
            result.Rows = new List<List<string>> { new List<string> { count.ToString() } };
            return result;
        }

        private async Task<QueryResult> GetUserStatsAsync(Dictionary<string, object> parameters, string userId)
        {
            bool myStats = parameters.TryGetValue("my", out var val) && (val is bool b && b || val.ToString() == "true");
            string? targetUserId = myStats ? userId : parameters.GetValueOrDefault("userid")?.ToString();

            if (string.IsNullOrEmpty(targetUserId))
                targetUserId = userId;

            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == targetUserId && e.IsActive)
                .Include(e => e.Course)
                .ToListAsync();

            var courseIds = enrollments.Select(e => e.CourseId).ToList();

            var completedProgress = await _context.LessonProgresses
                .Where(lp => lp.UserId == targetUserId && lp.IsCompleted)
                .CountAsync();

            var totalLessons = await _context.Lessons
                .Where(l => courseIds.Contains(l.Section.CourseId))
                .CountAsync();

            var certificates = await _context.Certificates
                .Where(c => c.UserId == targetUserId)
                .CountAsync();

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == targetUserId);

            var result = new QueryResult();
            result.Columns = new List<string> { "Stat", "Value" };
            result.Rows = new List<List<string>>
            {
                new() { "Khóa học đã đăng ký", enrollments.Count.ToString() },
                new() { "Khóa học đang học", enrollments.Count.ToString() },
                new() { "Bài học hoàn thành", completedProgress.ToString() + "/" + totalLessons.ToString() },
                new() { "Chứng chỉ đạt được", certificates.ToString() },
                new() { "Số dư ví", (wallet?.Balance ?? 0).ToString("N0") + " VNĐ" }
            };
            return result;
        }

        private async Task<QueryResult> GetUserProgressAsync(Dictionary<string, object> parameters, string userId)
        {
            bool myProgress = parameters.TryGetValue("my", out var val) && (val is bool b && b || val.ToString() == "true");
            string? targetUserId = myProgress ? userId : parameters.GetValueOrDefault("userid")?.ToString();

            if (string.IsNullOrEmpty(targetUserId))
                targetUserId = userId;

            var enrollments = await _context.Enrollments
                .Where(e => e.UserId == targetUserId && e.IsActive)
                .Include(e => e.Course)
                .ToListAsync();

            var result = new QueryResult();

            if (enrollments.Any())
            {
                var progressList = new List<List<string>>();

                foreach (var enrollment in enrollments)
                {
                    var courseId = enrollment.CourseId;
                    var totalLessons = await _context.Lessons
                        .Where(l => l.Section.CourseId == courseId)
                        .CountAsync();

                    var completedLessons = await _context.LessonProgresses
                        .Where(lp => lp.UserId == targetUserId && lp.Lesson.Section.CourseId == courseId && lp.IsCompleted)
                        .CountAsync();

                    var percent = totalLessons > 0 ? (completedLessons * 100 / totalLessons) : 0;

                    progressList.Add(new List<string>
                    {
                        enrollment.Course.Title,
                        completedLessons + "/" + totalLessons,
                        percent + "%"
                    });
                }

                result.Columns = new List<string> { "Course", "Completed/Total", "Progress" };
                result.Rows = progressList;
            }

            return result;
        }

        // ==================== REVIEW METHODS ====================

        private async Task<QueryResult> ListReviewsAsync(Dictionary<string, object> parameters, int? limit)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Course)
                .AsQueryable();

            if (parameters.TryGetValue("courseid", out var courseIdObj) && Guid.TryParse(courseIdObj.ToString(), out var courseId))
                query = query.Where(r => r.CourseId == courseId);

            if (limit.HasValue)
                query = query.Take(limit.Value);

            query = query.OrderByDescending(r => r.CreatedAt);

            var reviews = await query.ToListAsync();

            var result = new QueryResult();
            if (reviews.Any())
            {
                result.Columns = new List<string> { "User", "Course", "Rating", "Comment", "Date" };
                result.Rows = reviews.Select(r => new List<string>
                {
                    r.User.FullName,
                    r.Course.Title,
                    r.Rating + "/5",
                    r.Comment?.Length > 50 ? r.Comment[..50] + "..." : (r.Comment ?? ""),
                    r.CreatedAt.ToString("dd/MM/yyyy")
                }).ToList();
            }
            return result;
        }

        // ==================== WALLET METHODS ====================

        private async Task<QueryResult> GetWalletBalanceAsync(Dictionary<string, object> parameters, string userId)
        {
            bool myWallet = parameters.TryGetValue("my", out var val) && (val is bool b && b || val.ToString() == "true");
            string? targetUserId = myWallet ? userId : parameters.GetValueOrDefault("userid")?.ToString();

            if (string.IsNullOrEmpty(targetUserId))
                targetUserId = userId;

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == targetUserId);

            var result = new QueryResult();
            result.Columns = new List<string> { "Balance", "LastUpdated" };
            result.Rows = new List<List<string>>
            {
                new()
                {
                    (wallet?.Balance ?? 0).ToString("N0") + " VNĐ",
                    wallet?.UpdatedAt.ToString("dd/MM/yyyy HH:mm") ?? "N/A"
                }
            };
            return result;
        }

        private async Task<QueryResult> GetWalletTransactionsAsync(Dictionary<string, object> parameters, string userId, int? limit)
        {
            bool myTransactions = parameters.TryGetValue("my", out var val) && (val is bool b && b || val.ToString() == "true");
            string? targetUserId = myTransactions ? userId : parameters.GetValueOrDefault("userid")?.ToString();

            if (string.IsNullOrEmpty(targetUserId))
                targetUserId = userId;

            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == targetUserId);

            if (wallet == null)
                return new QueryResult { ErrorMessage = "Không tìm thấy ví của người dùng." };

            var query = _context.WalletTransactions
                .Where(t => t.WalletId == wallet.WalletId)
                .AsQueryable();

            if (limit.HasValue)
                query = query.Take(limit.Value);

            query = query.OrderByDescending(t => t.CreatedAt);

            var transactions = await query.ToListAsync();

            var result = new QueryResult();
            if (transactions.Any())
            {
                result.Columns = new List<string> { "Type", "Amount", "Description", "Date" };
                result.Rows = transactions.Select(t => new List<string>
                {
                    t.Type.ToString(),
                    (t.Type == WalletTransactionType.Purchase ? "-" : "+") + t.Amount.ToString("N0") + " VNĐ",
                    t.Description,
                    t.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                }).ToList();
            }
            return result;
        }

        // ==================== CATEGORY METHODS ====================

        private async Task<QueryResult> ListCategoriesAsync(int? limit)
        {
            var query = _context.Categories
                .Include(c => c.Courses)
                .AsQueryable();

            if (limit.HasValue)
                query = query.Take(limit.Value);

            var categories = await query.ToListAsync();

            var result = new QueryResult();
            if (categories.Any())
            {
                result.Columns = new List<string> { "Category", "CourseCount" };
                result.Rows = categories.Select(c => new List<string>
                {
                    c.CategoryName,
                    c.Courses.Count(c => !c.IsDeleted && c.Status == CourseStatus.Published).ToString()
                }).ToList();
            }
            return result;
        }

        // ==================== CERTIFICATE METHODS ====================

        private async Task<QueryResult> GetUserCertificatesAsync(Dictionary<string, object> parameters, string userId, int? limit)
        {
            bool myCerts = parameters.TryGetValue("my", out var val) && (val is bool b && b || val.ToString() == "true");
            string? targetUserId = myCerts ? userId : parameters.GetValueOrDefault("userid")?.ToString();

            if (string.IsNullOrEmpty(targetUserId))
                targetUserId = userId;

            var query = _context.Certificates
                .Include(c => c.Course)
                .Where(c => c.UserId == targetUserId)
                .AsQueryable();

            if (limit.HasValue)
                query = query.Take(limit.Value);

            query = query.OrderByDescending(c => c.IssuedAt);

            var certs = await query.ToListAsync();

            var result = new QueryResult();
            if (certs.Any())
            {
                result.Columns = new List<string> { "Course", "CertificateCode", "IssuedAt", "FilePath" };
                result.Rows = certs.Select(c => new List<string>
                {
                    c.Course.Title,
                    c.CertificateCode,
                    c.IssuedAt.ToString("dd/MM/yyyy"),
                    c.FileStoragePath ?? "Chưa có file"
                }).ToList();
            }
            return result;
        }

        // ==================== QUIZ METHODS ====================

        private async Task<QueryResult> GetUserQuizStatsAsync(Dictionary<string, object> parameters, string userId)
        {
            bool myStats = parameters.TryGetValue("my", out var val) && (val is bool b && b || val.ToString() == "true");
            string? targetUserId = myStats ? userId : parameters.GetValueOrDefault("userid")?.ToString();

            if (string.IsNullOrEmpty(targetUserId))
                targetUserId = userId;

            var attempts = await _context.QuizAttempts
                .Where(qa => qa.UserId == targetUserId)
                .ToListAsync();

            var passed = attempts.Count(a => a.IsPassed);
            var failed = attempts.Count(a => !a.IsPassed && a.AttemptedAt != default);
            var avgScore = attempts.Any() ? attempts.Where(a => a.Score.HasValue).Average(a => a.Score!.Value) : 0;

            var result = new QueryResult();
            result.Columns = new List<string> { "Stat", "Value" };
            result.Rows = new List<List<string>>
            {
                new() { "Tổng số lần làm quiz", attempts.Count.ToString() },
                new() { "Quiz đạt", passed.ToString() },
                new() { "Quiz chưa đạt", failed.ToString() },
                new() { "Điểm trung bình", avgScore.ToString("F1") + "%" }
            };
            return result;
        }

        // ==================== NOTIFICATION METHODS ====================

        private async Task<QueryResult> GetUserNotificationsAsync(Dictionary<string, object> parameters, string userId, int? limit)
        {
            bool myNotifs = parameters.TryGetValue("my", out var val) && (val is bool b && b || val.ToString() == "true");
            string? targetUserId = myNotifs ? userId : parameters.GetValueOrDefault("userid")?.ToString();

            if (string.IsNullOrEmpty(targetUserId))
                targetUserId = userId;

            var query = _context.Notifications
                .Where(n => n.UserId == targetUserId)
                .AsQueryable();

            if (limit.HasValue)
                query = query.Take(limit.Value);

            query = query.OrderByDescending(n => n.CreatedAt);

            var notifs = await query.ToListAsync();

            var result = new QueryResult();
            if (notifs.Any())
            {
                result.Columns = new List<string> { "Content", "Type", "Read", "Date" };
                result.Rows = notifs.Select(n => new List<string>
                {
                    n.Content.Length > 50 ? n.Content[..50] + "..." : n.Content,
                    n.Type.ToString(),
                    n.IsRead ? "Yes" : "No",
                    n.CreatedAt.ToString("dd/MM/yyyy HH:mm")
                }).ToList();
            }
            return result;
        }
    }
}
