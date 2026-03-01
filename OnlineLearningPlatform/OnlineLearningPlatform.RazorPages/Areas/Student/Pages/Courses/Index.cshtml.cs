using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Services.DTOs.Student.Request;
using OnlineLearningPlatform.Services.DTOs.Student.Response;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Student.Pages.Courses
{
    public class IndexModel : PageModel
    {
        private readonly IStudentService _studentService;

        public IndexModel(IStudentService studentService)
        {
            _studentService = studentService;
        }

        [BindProperty(SupportsGet = true)]
        public StudentCourseSearchRequest SearchRequest { get; set; } = new StudentCourseSearchRequest();

        public List<StudentCourseResponse> Courses { get; set; } = new();
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling(TotalCount / (double)SearchRequest.PageSize);

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var result = await _studentService.SearchCoursesAsync(SearchRequest.Keyword, userId, SearchRequest.PageNumber, SearchRequest.PageSize);
            Courses = result.Courses;
            TotalCount = result.TotalCount;
            return Page();
        }
    }
}
