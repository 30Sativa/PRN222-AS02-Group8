using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;
using System.Security.Claims;

namespace OnlineLearningPlatform.RazorPages.Areas.Teacher.Pages.Courses
{
    [Authorize(Roles = "Teacher")]
    public class IndexModel : PageModel
    {
        private readonly ICourseService _courseService;

        public IndexModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTitle { get; set; }

        // Chế độ xem: "mine" (mặc định) hoặc "all"
        [BindProperty(SupportsGet = true)]
        public string? ViewMode { get; set; }

        // Id teacher hiện tại để view quyết định nút chỉnh sửa
        public string? TeacherId { get; set; }

        public bool IsMyCoursesView =>
            string.IsNullOrWhiteSpace(ViewMode) ||
            ViewMode.Equals("mine", StringComparison.OrdinalIgnoreCase);

        public List<Course> Courses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                Courses = new List<Course>();
                return;
            }

            TeacherId = teacherId;

            List<Course> allCourses;

            if (IsMyCoursesView)
            {
                // Chỉ khóa học do chính teacher này tạo
                allCourses = await _courseService.GetMyCoursesAsync(teacherId);

                if (!string.IsNullOrWhiteSpace(SearchTitle))
                {
                    var keyword = SearchTitle.Trim().ToLower();
                    allCourses = allCourses
                        .Where(c => !string.IsNullOrWhiteSpace(c.Title) && c.Title.ToLower().Contains(keyword))
                        .ToList();
                }
            }
            else
            {
                // Xem tất cả khóa học đã xuất bản (mọi teacher) – tìm kiếm xử lý dưới DB
                allCourses = await _courseService.SearchPublishedCoursesAsync(SearchTitle);
            }

            Courses = allCourses
                .OrderBy(c => ParseCodeNumber(c.CourseCode))
                .ThenBy(c => c.CourseCode)
                .ToList();
        }

        private static int ParseCodeNumber(string? courseCode)
        {
            if (string.IsNullOrWhiteSpace(courseCode))
            {
                return int.MaxValue;
            }

            var digits = new string(courseCode.Where(char.IsDigit).ToArray());
            return int.TryParse(digits, out var number) ? number : int.MaxValue;
        }

        public async Task<JsonResult> OnGetStatusesAsync()
        {
            var teacherId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(teacherId))
            {
                return new JsonResult(Array.Empty<object>());
            }

            var courses = await _courseService.GetMyCoursesAsync(teacherId);
            var result = courses.Select(c => new
            {
                courseId = c.CourseId,
                status = c.Status.ToString()
            });

            return new JsonResult(result);
        }
    }
}
