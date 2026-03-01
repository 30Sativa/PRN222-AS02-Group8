using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using OnlineLearningPlatform.Models;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.User.Response;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages
{
    [Authorize(Roles = "Teacher")]
    public class DashboardModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _db;

        public DashboardModel(IUserService userService, ApplicationDbContext db)
        {
            _userService = userService;
            _db = db;
        }

        // ── User info ──────────────────────────────────────────────
        public UserInfoResponse? UserInfo { get; set; }

        // ── Stats ──────────────────────────────────────────────────
        public int TotalCourses     { get; set; }
        public int PublishedCourses { get; set; }
        public int PendingCourses   { get; set; }
        public int TotalStudents    { get; set; }
        public int TotalLessons     { get; set; }

        // ── Recent courses list (max 5) ────────────────────────────
        public List<TeacherCourseSummary> RecentCourses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return;

            // Load user info
            UserInfo = await _userService.GetUserInfoAsync(userId);

            // Stats — all via single efficient queries (no N+1)
            var courseIds = await _db.Courses
                .Where(c => c.TeacherId == userId && !c.IsDeleted)
                .Select(c => c.CourseId)
                .ToListAsync();

            TotalCourses = courseIds.Count;

            if (TotalCourses > 0)
            {
                PublishedCourses = await _db.Courses
                    .CountAsync(c => c.TeacherId == userId && !c.IsDeleted && c.Status == CourseStatus.Published);

                PendingCourses = await _db.Courses
                    .CountAsync(c => c.TeacherId == userId && !c.IsDeleted && c.Status == CourseStatus.Pending);

                TotalStudents = await _db.Enrollments
                    .Where(e => courseIds.Contains(e.CourseId) && e.IsActive)
                    .Select(e => e.UserId)
                    .Distinct()
                    .CountAsync();

                TotalLessons = await _db.Sections
                    .Where(s => courseIds.Contains(s.CourseId))
                    .SelectMany(s => s.Lessons)
                    .CountAsync();
            }

            // Recent courses with enrollment count
            RecentCourses = await _db.Courses
                .Where(c => c.TeacherId == userId && !c.IsDeleted)
                .OrderByDescending(c => c.CreatedAt)
                .Take(5)
                .Select(c => new TeacherCourseSummary
                {
                    CourseId        = c.CourseId,
                    Title           = c.Title,
                    Status          = c.Status.ToString(),
                    EnrollmentCount = c.Enrollments.Count(e => e.IsActive)
                })
                .ToListAsync();
        }
    }

    public class TeacherCourseSummary
    {
        public Guid   CourseId        { get; set; }
        public string Title           { get; set; } = string.Empty;
        public string Status          { get; set; } = string.Empty;
        public int    EnrollmentCount { get; set; }
    }
}