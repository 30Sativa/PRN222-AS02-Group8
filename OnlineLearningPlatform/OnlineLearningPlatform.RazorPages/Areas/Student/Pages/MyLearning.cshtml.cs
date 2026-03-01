using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.DTOs.Progress;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages
{
    public class MyLearningModel : PageModel
    {
        private readonly IEnrollmentService _enrollmentService;
        private readonly IProgressService _progressService;
        private readonly IOrderService _orderService; // Injected IOrderService

        public MyLearningModel(IEnrollmentService enrollmentService, IProgressService progressService, IOrderService orderService)
        {
            _enrollmentService = enrollmentService;
            _progressService = progressService;
            _orderService = orderService; // Assign IOrderService
        }

        public List<EnrolledCourseViewModel> EnrolledCourses { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null) return RedirectToPage("/Auth/Login");

            var enrollments = await _enrollmentService.GetMyEnrollmentsAsync(userId);

            foreach (var enrollment in enrollments)
            {
                var progress = await _progressService.GetCourseProgressAsync(userId, enrollment.CourseId);
                var resumeLessonId = await _progressService.GetResumeLessonIdAsync(userId, enrollment.CourseId);

                EnrolledCourses.Add(new EnrolledCourseViewModel
                {
                    Enrollment = enrollment,
                    Progress = progress,
                    ResumeLessonId = resumeLessonId
                });
            }

            return Page();
        }

        // Added OnPostRefundAsync method
        public async Task<IActionResult> OnPostRefundAsync(Guid courseId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToPage("/Auth/Login");

            var result = await _orderService.RefundCourseToWalletAsync(userId, courseId);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
            }
            else
            {
                TempData["ErrorMessage"] = result.Message;
            }

            return RedirectToPage();
        }
    }

    public class EnrolledCourseViewModel
    {
        public Enrollment Enrollment { get; set; } = default!;
        public CourseProgressDto Progress { get; set; } = default!;
        public int? ResumeLessonId { get; set; }
    }
}
