using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages
{
    public class DashboardModel : PageModel
    {
        private readonly IStudentService _studentService;
        public DashboardModel(IStudentService studentService) => _studentService = studentService;

        public IEnumerable<Course> Courses { get; set; } = new List<Course>();
        public string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "";

        public async Task OnGetAsync()
        {
            Courses = await _studentService.GetAllCoursesWithEnrollmentStatusAsync(CurrentUserId);
        }

        public async Task<IActionResult> OnPostEnrollAsync(Guid courseId)
        {
            if (string.IsNullOrEmpty(CurrentUserId)) return RedirectToPage("/Auth/Login", new { area = "" });

            await _studentService.EnrollInCourseAsync(CurrentUserId, courseId);

            return RedirectToPage();
        }
    }
}