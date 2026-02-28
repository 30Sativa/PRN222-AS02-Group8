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

        public MyLearningModel(IEnrollmentService enrollmentService, IProgressService progressService)
        {
            _enrollmentService = enrollmentService;
            _progressService = progressService;
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
    }

    public class EnrolledCourseViewModel
    {
        public Enrollment Enrollment { get; set; } = default!;
        public CourseProgressDto Progress { get; set; } = default!;
        public int? ResumeLessonId { get; set; }
    }
}
