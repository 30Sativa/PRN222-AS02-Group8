using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.RazorPages.Hubs;
using OnlineLearningPlatform.Services.DTOs.Progress;
using OnlineLearningPlatform.Services.Interface;
using OnlineLearningPlatform.Services.DTOs.Discussion;
using OnlineLearningPlatform.Services.DTOs.Review;
using System.Security.Claims;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages
{
    public class LearnCourseModel : PageModel
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IProgressService _progressService;
        private readonly IHubContext<ProgressHub> _progressHub;
        private readonly IDiscussionService _discussionService;
        private readonly IReviewService _reviewService;
        private readonly ICertificateService _certificateService;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<NotificationHub> _notificationHub;

        public LearnCourseModel(
            IEnrollmentService enrollmentService,
            IProgressService progressService,
            IHubContext<ProgressHub> progressHub,
            IDiscussionService discussionService,
            IReviewService reviewService,
            ICertificateService certificateService,
            INotificationService notificationService,
            IHubContext<NotificationHub> notificationHub)
        {
            _enrollmentService = enrollmentService;
            _progressService = progressService;
            _progressHub = progressHub;
            _discussionService = discussionService;
            _reviewService = reviewService;
            _certificateService = certificateService;
            _notificationService = notificationService;
            _notificationHub = notificationHub;
        }

        // ===== Bind properties =====

        public Course Course { get; set; } = default!;
        public Lesson? CurrentLesson { get; set; }
        public CourseProgressDto Progress { get; set; } = default!;
        public HashSet<int> CompletedLessonIds { get; set; } = new HashSet<int>();
        public int? ResumeLessonId { get; set; }

        public List<DiscussionTopicDto> Topics { get; set; } = new List<DiscussionTopicDto>();

        [BindProperty]
        public string NewTopicTitle { get; set; } = string.Empty;

        [BindProperty]
        public string NewReplyContent { get; set; } = string.Empty;

        public ReviewDto? MyReview { get; set; }

        [BindProperty]
        public int ReviewRating { get; set; }

        [BindProperty]
        public string ReviewComment { get; set; } = string.Empty;

        // ===== GET: /Student/LearnCourse?courseId=xxx&lessonId=yyy =====

        public async Task<IActionResult> OnGetAsync(Guid courseId, int? lessonId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Auth/Login");

            // Kiểm tra enrollment — chỉ học viên đã ghi danh mới vào được
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

            // Lấy danh sách Discussion (ưu tiên thuộc lesson hiện tại nếu muốn, hoặc lấy toàn bộ course)
            // Lấy top 20 discussion của khóa học
            Topics = await _discussionService.GetCourseTopicsAsync(courseId, 1, 20);

            // Fetch MyReview if already submitted
            MyReview = await _reviewService.GetUserReviewAsync(userId, courseId);

            return Page();
        }

        public async Task<IActionResult> OnPostCreateTopicAsync(Guid courseId, int? lessonId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Auth/Login");

            if (!string.IsNullOrWhiteSpace(NewTopicTitle))
            {
                await _discussionService.CreateTopicAsync(courseId, lessonId, NewTopicTitle, userId);
            }

            return RedirectToPage(new { courseId = courseId, lessonId = lessonId });
        }

        public async Task<IActionResult> OnPostCreateReplyAsync(Guid courseId, int? lessonId, Guid topicId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Auth/Login");

            if (!string.IsNullOrWhiteSpace(NewReplyContent))
            {
                await _discussionService.CreateReplyAsync(topicId, NewReplyContent, null, userId);
            }

            return RedirectToPage(new { courseId = courseId, lessonId = lessonId });
        }

        public async Task<IActionResult> OnPostSubmitReviewAsync(Guid courseId, int? lessonId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Auth/Login");

            if (ReviewRating < 1 || ReviewRating > 5)
            {
                ReviewRating = 5;
            }

            await _reviewService.SubmitReviewAsync(userId, courseId, ReviewRating, ReviewComment);
            
            return RedirectToPage(new { courseId = courseId, lessonId = lessonId });
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

                if (result.IsCourseCompleted)
                {
                    var certResult = await _certificateService.TryIssueCertificateAsync(userId, request.CourseId);
                    if (certResult.Success && !certResult.AlreadyIssued && certResult.Certificate != null)
                    {
                        var msg = $"Chúc mừng! Bạn đã hoàn thành {certResult.Certificate.CourseTitle} và nhận được chứng chỉ.";
                        await _notificationService.SendToUserAsync(userId, "CertificateIssued", msg, "/Student/Certificates");
                        
                        await _notificationHub.Clients.User(userId)
                            .SendAsync("ReceiveNotification", new 
                            { 
                                title = "Chứng chỉ mới",
                                message = msg,
                                url = "/Student/Certificates"
                            });
                    }
                }
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
