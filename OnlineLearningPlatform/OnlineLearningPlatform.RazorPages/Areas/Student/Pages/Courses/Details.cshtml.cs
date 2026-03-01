using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.Student.Response;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages.Courses
{
    public class DetailsModel : PageModel
    {
        private readonly IStudentService _studentService;

        public DetailsModel(IStudentService studentService)
        {
            _studentService = studentService;
        }

        public StudentCourseResponse Course { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var course = await _studentService.GetCourseDetailAsync(id, userId);

            if (course == null)
            {
                return NotFound();
            }

            Course = course;
            return Page();
        }
    }
}
