using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OnlineLearningPlatform.Models.Entities;
using OnlineLearningPlatform.Services.Interface;

namespace OnlineLearningPlatform.RazorPages.Areas.Admin.Pages.Courses
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ICourseService _courseService;

        public IndexModel(ICourseService courseService)
        {
            _courseService = courseService;
        }

        [BindProperty(SupportsGet = true)]
        public string? SearchTitle { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SearchTeacherName { get; set; }

        public List<Course> Courses { get; set; } = new();

        public async Task OnGetAsync()
        {
            var allCourses = await _courseService.GetAllForAdminAsync();
            var query = allCourses.AsQueryable();

            if (!string.IsNullOrWhiteSpace(SearchTitle))
            {
                var titleKeyword = SearchTitle.Trim().ToLower();
                query = query.Where(c => !string.IsNullOrWhiteSpace(c.Title)
                                         && c.Title.ToLower().Contains(titleKeyword));
            }

            if (!string.IsNullOrWhiteSpace(SearchTeacherName))
            {
                var teacherKeyword = SearchTeacherName.Trim().ToLower();
                query = query.Where(c => c.Teacher != null
                                         && !string.IsNullOrWhiteSpace(c.Teacher.FullName)
                                         && c.Teacher.FullName.ToLower().Contains(teacherKeyword));
            }

            Courses = query
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
    }
}
