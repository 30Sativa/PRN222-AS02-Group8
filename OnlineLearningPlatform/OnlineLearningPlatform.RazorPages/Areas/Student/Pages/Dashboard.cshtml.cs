using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.Progress;
using OnlineLearningPlatform.Services.DTOs.User.Response;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages
{
    [Authorize(Roles = "Student")]
    public class DashboardModel : PageModel
    {
        private readonly IUserService _userService;
        private readonly IEnrollmentService _enrollmentService;
        private readonly IProgressService _progressService;

        public DashboardModel(
            IUserService userService,
            IEnrollmentService enrollmentService,
            IProgressService progressService)
        {
            _userService        = userService;
            _enrollmentService  = enrollmentService;
            _progressService    = progressService;
        }

        // ── User info ──────────────────────────────────────────────
        public UserInfoResponse? UserInfo { get; set; }

        // ── Stats ──────────────────────────────────────────────────
        public int TotalEnrolled   { get; set; }
        public int CompletedCourses{ get; set; }
        public int InProgressCourses { get; set; }

        // ── Recent learning (max 4) ────────────────────────────────
        public List<StudentCourseItem> RecentCourses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return;

            UserInfo = await _userService.GetUserInfoAsync(userId);

            var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(userId);
            TotalEnrolled = enrollments.Count;

            var items = new List<StudentCourseItem>();
            foreach (var e in enrollments)
            {
                var progress = await _progressService.GetCourseProgressAsync(userId, e.CourseId);
                var resumeId = await _progressService.GetResumeLessonIdAsync(userId, e.CourseId);
                items.Add(new StudentCourseItem
                {
                    CourseId       = e.CourseId,
                    Title          = e.Course?.Title ?? "—",
                    TeacherName    = e.Course?.Teacher?.FullName ?? "—",
                    ThumbnailUrl   = e.Course?.ThumbnailUrl,
                    Progress       = progress,
                    ResumeLessonId = resumeId,
                    LastAccessed   = e.LastAccessedAt
                });
            }

            CompletedCourses  = items.Count(x => x.Progress.IsCompleted);
            InProgressCourses = items.Count(x => !x.Progress.IsCompleted && x.Progress.CompletedLessons > 0);

            // Sắp xếp: đang học trước, rồi mới nhất
            RecentCourses = items
                .OrderByDescending(x => !x.Progress.IsCompleted)
                .ThenByDescending(x => x.LastAccessed)
                .Take(4)
                .ToList();
        }
    }

    public class StudentCourseItem
    {
        public Guid               CourseId       { get; set; }
        public string             Title          { get; set; } = string.Empty;
        public string             TeacherName    { get; set; } = string.Empty;
        public string?            ThumbnailUrl   { get; set; }
        public CourseProgressDto  Progress       { get; set; } = default!;
        public int?               ResumeLessonId { get; set; }
        public DateTime?          LastAccessed   { get; set; }
    }
}