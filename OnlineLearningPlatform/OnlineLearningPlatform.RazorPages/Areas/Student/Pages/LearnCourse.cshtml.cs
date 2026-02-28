using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.RazorPages.Hubs;
using OnlineLearningPlatform.Services.DTOs.Progress;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages
{
    public class LearnCourseModel : PageModel
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IProgressService _progressService;
        private readonly IHubContext<ProgressHub> _progressHub;

        public LearnCourseModel(
            IEnrollmentService enrollmentService,
            IProgressService progressService,
            IHubContext<ProgressHub> progressHub)
        {
            _enrollmentService = enrollmentService;
            _progressService = progressService;
            _progressHub = progressHub;
        }

        // ===== Bind properties =====

        public Course Course { get; set; } = default!;
        public Lesson? CurrentLesson { get; set; }
        public CourseProgressDto Progress { get; set; } = default!;
        public HashSet<int> CompletedLessonIds { get; set; } = new();
        public int? ResumeLessonId { get; set; }

        // ===== GET: /Student/LearnCourse?courseId=xxx&lessonId=yyy =====

        public async Task<IActionResult> OnGetAsync(Guid courseId, int? lessonId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Auth/Login");

            // Kiểm tra enrollment
            var enrollment = await _enrollmentService.GetEnrollmentAsync(userId, courseId);
            if (enrollment == null)
                return RedirectToPage("/Student/Dashboard");

            Course = enrollment.Course;

            // Cập nhật LastAccessed (4.10)
            await _enrollmentService.UpdateLastAccessedAsync(userId, courseId);

            // Lấy progress
            Progress = await _progressService.GetCourseProgressAsync(userId, courseId);
            CompletedLessonIds = await _progressService.GetCompletedLessonIdsAsync(userId, courseId);

            // Resume feature (4.9): nếu không chỉ định lessonId → tự chọn bài chưa hoàn thành
            if (lessonId == null)
            {
                ResumeLessonId = await _progressService.GetResumeLessonIdAsync(userId, courseId);
                lessonId = ResumeLessonId;
            }

            // Tìm bài hiện tại
            if (lessonId != null)
            {
                CurrentLesson = Course.Sections
                    .SelectMany(s => s.Lessons)
                    .FirstOrDefault(l => l.LessonId == lessonId.Value && !l.IsDeleted);
            }

            // Nếu vẫn null → chọn bài đầu tiên
            if (CurrentLesson == null)
            {
                CurrentLesson = Course.Sections
                    .OrderBy(s => s.OrderIndex)
                    .SelectMany(s => s.Lessons.OrderBy(l => l.OrderIndex))
                    .FirstOrDefault(l => !l.IsDeleted);
            }

            return Page();
        }

        // ===== POST: AJAX mark lesson complete (4.7) =====

        public async Task<IActionResult> OnPostMarkCompleteAsync([FromBody] MarkCompleteRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return new JsonResult(new { success = false, message = "Chưa đăng nhập." });

            var result = await _progressService.MarkLessonCompleteAsync(userId, request.LessonId, request.CourseId);

            // SignalR: broadcast real-time progress update cho tất cả student đang xem cùng course
            if (result.Success)
            {
                await _progressHub.Clients.Group($"course_{request.CourseId}")
                    .SendAsync("LessonCompleted", new
                    {
                        lessonId = request.LessonId,
                        courseId = request.CourseId,
                        userId = userId,
                        userName = User.Identity?.Name ?? "",
                        percentComplete = result.PercentComplete,
                        completedLessons = result.CompletedLessons,
                        totalLessons = result.TotalLessons
                    });
            }

            return new JsonResult(new
            {
                success = result.Success,
                message = result.Message,
                percentComplete = result.PercentComplete,
                isCourseCompleted = result.IsCourseCompleted,
                completedLessons = result.CompletedLessons,
                totalLessons = result.TotalLessons
            });
        }

        // ===== POST: AJAX update watched seconds =====

        public async Task<IActionResult> OnPostUpdateWatchedAsync([FromBody] UpdateWatchedRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
                return new JsonResult(new { success = false });

            await _progressService.UpdateWatchedSecondsAsync(userId, request.LessonId, request.WatchedSeconds);
            return new JsonResult(new { success = true });
        }
    }

    // ===== Request models =====

    public class MarkCompleteRequest
    {
        public int LessonId { get; set; }
        public Guid CourseId { get; set; }
    }

    public class UpdateWatchedRequest
    {
        public int LessonId { get; set; }
        public int WatchedSeconds { get; set; }
    }
}
